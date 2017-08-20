using System.Collections.Generic;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class CubeGenerator : BenchmarkGenerator
    {
        private ContinuousUniform _uniformDistributionForPositives;
        private ContinuousUniform _uniformDistributionForNegatives;

        public override IList<Point> Generate(int dimensions, double d, int positives, int negatives = 0)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;

            _uniformDistributionForPositives = new ContinuousUniform(1, dimensions + dimensions * d, RandomSource);
            _uniformDistributionForNegatives = new ContinuousUniform(dimensions - dimensions * d, dimensions + 2 * dimensions * d, RandomSource);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                _uniformDistributionForPositives.Samples(samples);

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

                _uniformDistributionForNegatives.Samples(samples);

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
    }
}