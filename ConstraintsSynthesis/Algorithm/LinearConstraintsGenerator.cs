using System;
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

        public static IEnumerable<LinearConstraint> GenerateRandomLinearConstraints(Cluster cluster,
            int constraintsCount)
        {
            const int DefaultStdDev = 30;

            for (var i = 0; i < constraintsCount; i++)
            {
                var randomPointCoordinates = cluster.Points[Random.Next(cluster.Size)].Coordinates;
                var coefficients = new double[randomPointCoordinates.Length];

                Normal.Samples(Random, coefficients, 0.0, DefaultStdDev);

                var absoluteTerm = randomPointCoordinates.Zip(coefficients, (p, c) => p*c).Sum();

                yield return new LinearConstraint(coefficients, absoluteTerm);
            }
        }

        public static IEnumerable<LinearConstraint> GenerateRandomLinearConstraintsCrossingOrigin(Cluster cluster,
            int constraintsCount)
        {
            for (var i = 0; i < constraintsCount; i++)
            {
                var randomPointCoordinates = cluster.Points[Random.Next(cluster.Size)].Coordinates;
                var coefficients = new double[randomPointCoordinates.Length];

                coefficients[0] = Normal.Sample(Random, 0.0, 5);

                for (var j = 1; j < coefficients.Length - 1; j++)
                {
                    var deviation = randomPointCoordinates.Zip(coefficients, (p, c) => p * c).Sum();

                    coefficients[i] = Normal.Sample(Random, -deviation, Math.Abs(deviation) / (3 * randomPointCoordinates[i]));
                }

                coefficients[coefficients.Length - 1] =
                    -randomPointCoordinates.Zip(coefficients, (p, c) => p * c).Sum() /
                    randomPointCoordinates[coefficients.Length - 1];

                var sign = Random.Next(2);

                yield return new LinearConstraint(coefficients, 0) {Sign = (Inequality)sign};
            }
        }
    }
}