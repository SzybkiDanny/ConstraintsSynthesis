using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace ConstraintsSynthesis.Algorithm
{
    public static class LinearConstraintsGenerator
    {
        private static readonly MersenneTwister Random = new MersenneTwister(Program.Seed);

        public static IEnumerable<LinearConstraint> GenerateRandomLinearConstraints(IList<Point> data,
            int constraintsCount)
        {
            for (var i = 0; i < constraintsCount; i++)
            {
                var randomPointCoordinates = data[Random.Next(data.Count)].Coordinates;
                var coefficients = new double[randomPointCoordinates.Length];

                Normal.Samples(Random, coefficients, 0.0, 1.0);

                var absoluteTerm = randomPointCoordinates.Zip(coefficients, (p, c) => p*c).Sum();

                yield return new LinearConstraint(coefficients, absoluteTerm);
            }
        }
    }
}