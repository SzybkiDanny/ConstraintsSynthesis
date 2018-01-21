using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Accord.Math.Random;
using Accord.Statistics.Analysis;
using CommandLine;
using ConstraintsSynthesis.Algorithm;
using ConstraintsSynthesis.Benchmarks;
using ConstraintsSynthesis.Model;
using ConstraintsSynthesis.Visualization;
using ExperimentDatabase;
using MethodTimer;
using ConstraintMetric = ConstraintsSynthesis.Model.Enums.ConstraintMetric;

namespace ConstraintsSynthesis
{
    internal class Program
    {
        public static int Seed;
        static readonly Process Process = Process.GetCurrentProcess();

        private static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now}: {string.Join(" ", args)}");

            using (var database = new Database("stats.sqlite"))
            {
                using (var experiment = database.NewExperiment())
                {
                    try
                    {
                        var options = new Options();

                        if (!Parser.Default.ParseArguments(args, options))
                            return;
                        MethodTimeLogger.LogLevel = options.LogLevel;
                        Generator.Seed = Seed = options.Seed ?? DateTime.Now.Millisecond;
                        options.Seed = Seed;

                        foreach (var propertyName in typeof(Options).GetProperties().Select(p => p.Name))
                        {
                            experiment[propertyName] = typeof(Options).GetProperty(propertyName).GetValue(options);
                        }

                        IList<Point> trainingPoints;
                        IList<Point> testPoints;

                        if (string.IsNullOrEmpty(options.InputFile))
                        {
                            var generatorType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(s => s.GetTypes())
                                .FirstOrDefault(
                                    p =>
                                        typeof(BenchmarkGenerator).IsAssignableFrom(p) &&
                                        p.Name.StartsWith(options.Benchmark));

                            var benchmarkGenerator = Activator.CreateInstance(generatorType) as BenchmarkGenerator;

                            trainingPoints = benchmarkGenerator.Generate(options.Dimensions, options.d, options.k,
                                options.TrainingDataSize, 0);

                            testPoints = options.TestSize > 0
                                ? benchmarkGenerator.Generate(options.Dimensions, options.d, options.k, options.TestSize)
                                : benchmarkGenerator.Generate(options.Dimensions, options.d, options.k, options.TestPositiveSize,
                                    options.TestNegativeSize);

                            experiment["testPointsPositive"] = testPoints.Count(p => p.Label);
                            experiment["testPointsNegative"] = testPoints.Count(p => !p.Label);
                        }
                        else
                        {
                            var inputData = new Data();

                            inputData.Load(options.InputFile, options.Delimiter);

                            trainingPoints = inputData.Points.GetRange(0, options.TrainingDataSize);
                            testPoints = inputData.Points.GetRange(options.TrainingDataSize,
                                Math.Min(inputData.Points.Count - options.TrainingDataSize,
                                    options.TestPositiveSize + options.TestNegativeSize));
                        }

                        XMeans xmeans = null;

                        experiment["clusteringTime"] = MeasureTime(() =>
                        {
                            xmeans = new XMeans(options.MinK, options.NormalizeData, options.EnforceSingleCluster);
                            xmeans.Fit(trainingPoints);
                        });

                        experiment["clusters"] = xmeans.Clusters.Count;

                        var clusters = xmeans.Clusters;
                        var solutions = clusters.Select((c, i) => new Solution(c, i)).ToArray();

                        List<string> model = new List<string>();

                        foreach (var solution in solutions)
                        {
                            string[] constraints;

                            experiment["constraintsGenerationTime"] = MeasureTime(() =>
                            {
                                solution.GenerateInitialSolution()
                                    .GenerateRandomConstraints(options.ConstraintsGeneration,
                                        options.RandomConstraintsToGenerate,
                                        options.OptimizeSign, options.OptimizeCoefficients);
                            });

                            experiment["redundantConstraintsRemovingTime"] = MeasureTime(() =>
                            {
                                solution.RemoveRedundantConstraints(ConstraintMetric.MostUnsatisfied,
                                    options.SamplingSize,
                                    options.MariginExpansion, options.AngleSimilarity, reduceSatisfiedPoints: true);
                            });

                            experiment["negativePointsGenerationTime"] = MeasureTime(() =>
                            {
                                solution.GenerateNegativePointsFromClusterDistribution(options.Quantile,
                                    options.GeneratedNegativesSetSize);
                            });

                            solution.GenerateReadableSolution(out constraints);

                            model.AddRange(constraints);
                        }

                        for (var i = 0; i < model.Count; i++)
                        {
                            experiment.NewChildDataSet("constraints")["constraint"] = model[i];
                        }
                        experiment.NewChildDataSet("constraints")["constraint"] =
                            $"{string.Join(" + ", Enumerable.Range(0, solutions.Length).Select(b => $"b{b}"))} = 1";


                        experiment["statsGenerationTime"] = MeasureTime(() =>
                        {
                            GenerateStats(testPoints, solutions, experiment);
                        });

                        if (!options.VisualizationCreation)
                            return;

                        Application.EnableVisualStyles();
                        Application.Run(new VisualizationApplicationContext(solutions, options.Dimensions));
                    }
                    catch (Exception e)
                    {
                        experiment["error"] = e.Message;
                        throw;
                    }
                    finally
                    {
                        experiment.Save();
                    }
                }
            }
        }

        [Time("Generating stats")]
        public static void GenerateStats(IList<Point> testPoints, Solution[] solutions, Dictionary<string, object> stats)
        {
            var constraints = solutions.SelectMany(s => s.Constraints).ToList();
            var relevantConstraints = constraints.Where(c => !c.IsMarkedRedundant).ToList();

            stats["generatedConstraints"] = constraints.Count();
            stats["relevantConstraints"] = relevantConstraints.Count();
            stats["relevantTerms"] = relevantConstraints.Sum(x => x.Terms.Count);

            var predicted = testPoints.Select(p =>solutions.Any(s => s.Constraints.Where(c => !c.IsMarkedRedundant).All(c => c.IsSatisfying(p)))).ToArray();
            var confusionMatrix = new ConfusionMatrix(predicted, testPoints.Select(p => p.Label).ToArray());

            stats["truePositives"] = confusionMatrix.TruePositives;
            stats["falsePositives"] = confusionMatrix.FalsePositives;
            stats["trueNegatives"] = confusionMatrix.TrueNegatives;
            stats["falseNegatives"] = confusionMatrix.FalseNegatives;
            stats["accuracy"] = confusionMatrix.Accuracy;
            stats["fscore"] = confusionMatrix.FScore;
            stats["precision"] = confusionMatrix.Precision;
            stats["recall"] = confusionMatrix.Recall;
            stats["mcc"] = confusionMatrix.MatthewsCorrelationCoefficient;
        }

        public static TimeSpan GetTotalProcessTime()
        {
            Process.Refresh();
            return Process.TotalProcessorTime;
        }

        public static TimeSpan MeasureTime(Action action)
        {
            var start = GetTotalProcessTime();

            action?.Invoke();

            return GetTotalProcessTime() - start;
        }
    }
}