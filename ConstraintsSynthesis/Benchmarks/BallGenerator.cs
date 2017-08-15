using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;

namespace ConstraintsSynthesis.Benchmarks
{
    public class BallGenerator
    {
        private ContinuousUniform _uniformDistributionForPositives;
        private ContinuousUniform _uniformDistributionForNegatives;

        public IList<Point> Generate(int dimensions, double d, int positives, int negatives = 0)
        {
            var result = new List<Point>(positives + negatives);
            var generatedPositives = 0;
            var generatedNegatives = 0;

            _uniformDistributionForPositives = new ContinuousUniform(1 - d, d + dimensions);
            _uniformDistributionForNegatives = new ContinuousUniform(1 - 2 * d, dimensions + 2 * d);

            while (generatedPositives < positives)
            {
                var samples = new double[dimensions];

                _uniformDistributionForPositives.Samples(samples);

                if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(Convert.ToDouble).ToArray(), samples) > d)
                    continue;

                generatedPositives++;
                result.Add(new Point(samples) { Label = true }); 
            }

            while (generatedNegatives < negatives)
            {
                var samples = new double[dimensions];

                _uniformDistributionForNegatives.Samples(samples);

                if (Distance.Euclidean(Enumerable.Range(1, dimensions).Select(Convert.ToDouble).ToArray(), samples) <= d)
                    continue;

                generatedNegatives++;
                result.Add(new Point(samples) { Label = false });
            }

            return result;
        }
    }
}