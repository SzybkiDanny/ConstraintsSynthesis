using System;
using System.Collections.Generic;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class SimplexGenerator : BenchmarkGenerator
    {
        private ContinuousUniform _uniformDistribution;

        public override IList<Point> Generate(int dimensions, double d, int positives, int negatives = 0)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;

            _uniformDistribution = new ContinuousUniform(-1, 2 + d, RandomSource);


            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];
                var isPositive = true;

                _uniformDistribution.Samples(samples);

                if (samples.Sum() > d)
                    continue;

                for (var i = 1; i <= dimensions; i++)
                {
                    for (int j = i + 1; j < dimensions; j++)
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

                _uniformDistribution.Samples(samples);

                if (samples.Sum() < d)
                    continue;

                generatedNegatives++;
                result.Add(new Point(samples) { Label = false });
            }

            return result;
        }
    }
}