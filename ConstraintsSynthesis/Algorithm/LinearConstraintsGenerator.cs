using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace ConstraintsSynthesis.Algorithm
{
    internal class LinearConstraintsGenerator
    {
        private readonly MersenneTwister _random = new MersenneTwister();

        public IList<LinearConstraint> GenerateRandomLinearConstraints(IList<Point> data,
            int constraintsCount)
        {
            var result = new List<LinearConstraint>(constraintsCount);

            for (var i = 0; i < constraintsCount; i++)
            {
                var randomPointCoordinates = data[_random.Next(data.Count)].Coordinates;
                var coefficients = new double[randomPointCoordinates.Length];

                Normal.Samples(_random, coefficients, 0.0, 1.0);

                var absoluteTerm = randomPointCoordinates.Zip(coefficients, (p, c) => p*c).Sum();

                result.Add(new LinearConstraint(coefficients, absoluteTerm));
            }

            return result;
        }
    }
}