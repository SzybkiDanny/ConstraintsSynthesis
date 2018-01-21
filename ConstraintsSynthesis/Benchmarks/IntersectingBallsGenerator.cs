using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class IntersectingBallsGenerator : BenchmarkGenerator
    {
        private readonly double _edgeCoefficient = 2 * Math.Sqrt(6) / Math.PI;

        public override IList<Point> Generate(int dimensions, double d, int k, int positives, int negatives)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;
            //var uniformDistributionForPositives = new ContinuousUniform(1 - d, 2 * d + dimensions, RandomSource);
            var uniformDistributionForPositives = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d + (2 * Math.Sqrt(6) * (k - 1) * d) / Math.PI, RandomSource);
            var uniformDistributionForNegatives = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d + (2 * Math.Sqrt(6) * (k - 1) * d) / Math.PI, RandomSource);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistributionForPositives.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    isPositive = true;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(c => Convert.ToDouble(c) + _edgeCoefficient * d * (j - 1) / c).ToArray(), samples) <= d)
                                continue;

                        isPositive = false;
                        break;
                    }

                    if (isPositive)
                        break;
                }

                if (!isPositive)
                    continue;

                generatedPositives++;
                result.Add(new Point(samples) {Label = true});
            }

            while (generatedNegatives < negatives)
            {
                var samples = new double[dimensions];
                var isNegative = false;

                uniformDistributionForNegatives.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    isNegative = false;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(c => Convert.ToDouble(c) + _edgeCoefficient * d * (j - 1) / c).ToArray(), samples) <= d)
                            continue;

                        isNegative = true;
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
            var uniformDistribution = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d + (2 * Math.Sqrt(6) * (k - 1) * d) / Math.PI, RandomSource);

            while (generated++ < total)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistribution.Samples(samples);

                for (var j = 1; j <= k; j++)
                {
                    isPositive = true;

                    for (var i = 1; i <= dimensions; i++)
                    {
                        if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(c => Convert.ToDouble(c) + _edgeCoefficient * d * (j - 1) / c).ToArray(), samples) <= d)
                            continue;

                        isPositive = false;
                        break;
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