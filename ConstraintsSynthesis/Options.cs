using CommandLine;
using CommandLine.Text;
using ConstraintsSynthesis.Model.Enums;

namespace ConstraintsSynthesis
{
    internal class Options
    {
        [Option("outputFile", Required = false,
            HelpText = "Output file name.")]
        public string OutputFile { get; set; }

        [Option("inputFile", Required = false,
            HelpText = "Input file name.")]
        public string InputFile { get; set; }

        [Option("inputDelimiter", DefaultValue = " ", Required = false,
            HelpText = "Data delimiter in input file.")]
        public string Delimiter { get; set; }

        [Option("seed", Required = false, HelpText = "Seed for all random operations.")]
        public int? Seed { get; set; }

        [Option("logLevel", DefaultValue = TimeLogLevel.General, Required = false,
            HelpText = "Seed for all random operations.")]
        public TimeLogLevel LogLevel { get; set; }

        [Option("visualizationCreation", DefaultValue = Boolean.False, Required = false,
            HelpText = "Creating solution visualization.")]
        public Boolean _visualizationCreation { private get; set; }

        public bool VisualizationCreation => _visualizationCreation.Equals(Boolean.True);

        [Option("benchmark", DefaultValue = "Simplex", Required = false,
            HelpText = "Benchmark type to be used.")]
        public string Benchmark { get; set; }

        [Option("dimensions", DefaultValue = 2, Required = false,
            HelpText = "Number of dimensions for benchmark.")]
        public int Dimensions { get; set; }

        [Option("d", DefaultValue = 2.7, Required = false,
            HelpText = "d constant for benchmark generation.")]
        public double d { get; set; }

        [Option("k", DefaultValue = 1, Required = false,
            HelpText = "k constant for benchmark generation.")]
        public int k { get; set; }

        [Option("trainingSize", DefaultValue = 1000, Required = false,
            HelpText = "Training data set size.")]
        public int TrainingDataSize { get; set; }

        [Option("testSize", DefaultValue = 0, Required = false,
            HelpText = "Test data set size.")]
        public int TestSize { get; set; }

        [Option("testPositiveSize", DefaultValue = 10000, Required = false,
            HelpText = "Positive test data set size.")]
        public int TestPositiveSize { get; set; }

        [Option("testNegativeSize", DefaultValue = 10000, Required = false,
            HelpText = "Negative test data set size.")]
        public int TestNegativeSize { get; set; }

        [Option("enforceSingleCluster", DefaultValue = Boolean.False, Required = false,
            HelpText = "Enforce not using clustering.")]
        public Boolean _enforceSingleCluster { private get; set; }

        public bool EnforceSingleCluster => _enforceSingleCluster.Equals(Boolean.True);

        [Option("minClusters", DefaultValue = 1, Required = false,
            HelpText = "Minimum number of result clusters.")]
        public int MinK { get; set; }

        [Option("normalizeData", DefaultValue = Boolean.True, Required = false,
            HelpText = "Normalizing data before clustering.")]
        public Boolean _normalizeData { private get; set; }

        public bool NormalizeData => _normalizeData.Equals(Boolean.True);

        [Option("constraintsGeneration", DefaultValue = ConstraintsGeneration.CrossingRandomPoint, Required = false,
            HelpText = "Constraints generation algorithm type.")]
        public ConstraintsGeneration ConstraintsGeneration { get; set; }

        [Option("optimizeSign", DefaultValue = Boolean.True, Required = false,
            HelpText = "Sign optimization.")]
        public Boolean _optimizeSign { private get; set; }

        public bool OptimizeSign => _optimizeSign.Equals(Boolean.True);

        [Option("optimizeCoefficients", DefaultValue = Boolean.True, Required = false,
            HelpText = "Coefficients optimization.")]
        public Boolean _optimizeCoefficients { private get; set; }

        public bool OptimizeCoefficients => _optimizeCoefficients.Equals(Boolean.True);

        [Option("randomConstraintsToGenerate", DefaultValue = 100, Required = false,
            HelpText = "Number of random constraints to generate.")]
        public int RandomConstraintsToGenerate { get; set; }

        [Option("angleSimilarity", DefaultValue = 5, Required = false,
            HelpText = "Maximum angle between constraints to be similar.")]
        public double AngleSimilarity { get; set; }

        [Option("mariginExpansion", DefaultValue = 0.5, Required = false,
            HelpText = "Expansion increase for sampling.")]
        public double MariginExpansion { get; set; }

        [Option("samplingSize", DefaultValue = 1000, Required = false,
            HelpText = "Size of random sample used to reduce redundant constraints.")]
        public int SamplingSize { get; set; }

        [Option("quantile", DefaultValue = 0.999, Required = false,
            HelpText = "Quantile to be used for negative points generation.")]
        public double Quantile { get; set; }

        [Option("generatedNegativesSetSize", DefaultValue = 0, Required = false,
            HelpText = "Size of randomly generated set of negative points.")]
        public int GeneratedNegativesSetSize { get; set; }

        [HelpOption]
        public string GetUsage() => HelpText.AutoBuild(this,
            current => HelpText.DefaultParsingErrorsHandler(this, current));

        public enum Boolean
        {
            False,
            True
        }
    }
}