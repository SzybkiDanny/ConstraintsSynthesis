using System;
using System.Collections.Generic;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class IntersectingSimplicesGenerator : BenchmarkGenerator
    {
        public override IList<Point> Generate(int dimensions, double d, int k, int positives, int negatives)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;
            var uniformDistribution = new ContinuousUniform(-1, 2 * k + d, RandomSource);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = false;

                uniformDistribution.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    if (samples.Sum() > d * j)
                        continue;

                    isPositive = true;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        for (var l = i + 1; l <= dimensions; l++)
                        {
                            if (samples[i - 1] / Math.Tan(Math.PI / 12) - samples[l - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2 &&
                                samples[l - 1] / Math.Tan(Math.PI / 12) - samples[i - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2)
                                continue;

                            isPositive = false;
                            break;
                        }
                    }

                    if (isPositive)
                        break;
                }

                if (!isPositive)
                    continue;

                generatedPositives++;
                result.Add(new Point(samples) { Label = true }); 
            }

            while (generatedNegatives < negatives)
            {
                var samples = new double[dimensions];
                var isNegative = false;

                uniformDistribution.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    isNegative = false;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        for (var l = i + 1; l <= dimensions; l++)
                        {
                            if (samples[i - 1] / Math.Tan(Math.PI / 12) - samples[l - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2 &&
                                samples[l - 1] / Math.Tan(Math.PI / 12) - samples[i - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2 &&
                                samples.Sum() < d * j)
                                continue;

                            isNegative = true;
                            break;
                        }

                        if (isNegative)
                            break;
                    }

                    if (!isNegative)
                        break;
                }

                if (!isNegative)
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
            var uniformDistribution = new ContinuousUniform(-1, 2 * k + d, RandomSource);

            while (generated++ < total)
            {
                var samples = new double[dimensions];
                var isPositive = false;

                uniformDistribution.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    if (samples.Sum() > d * j)
                        continue;

                    isPositive = true;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        for (var l = i + 1; l <= dimensions; l++)
                        {
                            if (samples[i - 1] / Math.Tan(Math.PI / 12) - samples[l - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2 &&
                                samples[l - 1] / Math.Tan(Math.PI / 12) - samples[i - 1] * Math.Tan(Math.PI / 12) >= 2 * j - 2)
                                continue;

                            isPositive = false;
                            break;
                        }
                    }

                    if (isPositive)
                        break;
                }

                result.Add(new Point(samples) { Label = isPositive });
            }

            return result;
        }
    }
}