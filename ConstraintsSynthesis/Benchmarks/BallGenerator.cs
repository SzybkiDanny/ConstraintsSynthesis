using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class BallGenerator : BenchmarkGenerator
    {
        public override IList<Point> Generate(int dimensions, double d, int k, int positives, int negatives)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;
            var uniformDistributionForPositives = new ContinuousUniform(1 - d, d + dimensions, RandomSource);
            var uniformDistributionForNegatives = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d, RandomSource);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];

                uniformDistributionForPositives.Samples(samples);

                if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(Convert.ToDouble).ToArray(), samples) > d)
                    continue;

                generatedPositives++;
                result.Add(new Point(samples) { Label = true }); 
            }

            while (generatedNegatives < negatives)
            {
                var samples = new double[dimensions];

                uniformDistributionForNegatives.Samples(samples);

                if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(Convert.ToDouble).ToArray(), samples) <= d)
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
            var uniformDistribution = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d, RandomSource);

            while (generated++ < total)
            {
                var samples = new double[dimensions];

                uniformDistribution.Samples(samples);

                var label = Distance.Euclidean(Enumerable.Range(1, dimensions).Select(Convert.ToDouble).ToArray(), samples) <= d;

                result.Add(new Point(samples) { Label = label });
            }

            return result;
        }
    }
}