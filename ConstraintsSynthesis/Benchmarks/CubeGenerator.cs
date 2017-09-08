using System.Collections.Generic;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class CubeGenerator : BenchmarkGenerator
    {
        public override IList<Point> Generate(int dimensions, double d, int positives, int negatives)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;
            var uniformDistributionForPositives = new ContinuousUniform(1, dimensions + dimensions * d, RandomSource);
            var uniformDistributionForNegatives = new ContinuousUniform(dimensions - dimensions * d, dimensions + 2 * dimensions * d, RandomSource);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistributionForPositives.Samples(samples);

                for (var i = 1; i <= dimensions; i++)
                {
                    if (samples[i - 1] < i || samples[i - 1] > i + i * d)
                    {
                        isPositive = false;
                        break;
                    }
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

                for (var i = 1; i <= dimensions; i++)
                {
                    if (samples[i - 1] < i || samples[i - 1] > i + i * d)
                    {
                        isNegative = true;
                        break;
                    }
                }

                if (!isNegative)
                    continue;

                generatedNegatives++;
                result.Add(new Point(samples) { Label = false });
            }

            return result;
        }

        public override IList<Point> Generate(int dimensions, double d, int total)
        {
            var result = new List<Point>(total);
            var generated = 0;
            var uniformDistribution = new ContinuousUniform(dimensions - dimensions * d, dimensions + 2 * dimensions * d, RandomSource);

            while (generated++ < total)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                uniformDistribution.Samples(samples);

                for (var i = 1; i <= dimensions; i++)
                {
                    if (samples[i - 1] < i || samples[i - 1] > i + i * d)
                    {
                        isPositive = false;
                        break;
                    }
                }

                result.Add(new Point(samples) { Label = isPositive });
            }

            return result;
        }
    }
}