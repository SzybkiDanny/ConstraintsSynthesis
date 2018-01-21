using System;
using System.Collections.Generic;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class SimplexGenerator : BenchmarkGenerator
    {
        public override IList<Point> Generate(int dimensions, double d, int k, int positives, int negatives)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;
            var uniformDistribution = new ContinuousUniform(-1, 2 + d, RandomSource);


            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistribution.Samples(samples);

                if (samples.Sum() > d)
                    continue;

                for (var i = 1; i <= dimensions; i++)
                {
                    for (var j = i + 1; j < dimensions; j++)
                    {
                        if (samples[i - 1] / Math.Tan(Math.PI / 12) - samples[j - 1] * Math.Tan(Math.PI / 12) < 0 ||
                            samples[j - 1] / Math.Tan(Math.PI / 12) - samples[i - 1] * Math.Tan(Math.PI / 12) < 0)
                        {
                            isPositive = false;
                            break;
                        }
                    }
                }

                if (!isPositive)
                    continue;

                generatedPositives++;
                result.Add(new Point(samples) { Label = true }); 
            }

            while (generatedNegatives < negatives)
            {
                var samples = new double[dimensions];

                uniformDistribution.Samples(samples);

                if (samples.Sum() < d)
                    continue;

                generatedNegatives++;
                result.Add(new Point(samples) { Label = false });
            }

            return result;
        }

        public override IList<Point> Generate(int dimensions, double d, int k, int total)
        {
            var result = new List<Point>(total);
            var generated = 0;
            var uniformDistribution = new ContinuousUniform(-1, 2 + d, RandomSource);

            while (generated++ < total)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistribution.Samples(samples);

                if (samples.Sum() > d)
                    isPositive = false;
                else
                    for (var i = 1; i <= dimensions; i++)
                    {
                        for (var j = i + 1; j < dimensions; j++)
                        {
                            if (samples[i - 1] / Math.Tan(Math.PI / 12) - samples[j - 1] * Math.Tan(Math.PI / 12) < 0 ||
                                samples[j - 1] / Math.Tan(Math.PI / 12) - samples[i - 1] * Math.Tan(Math.PI / 12) < 0)
                            {
                                isPositive = false;
                            }
                        }
                    }

                result.Add(new Point(samples) { Label = isPositive });
            }

            return result;
        }
    }
}