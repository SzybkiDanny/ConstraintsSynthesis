using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using TestDataGenerator.Model;

namespace TestDataGenerator.Generators
{
    internal class SphereGenerator : IShapeGenerator
    {
        private readonly Normal _normalDistribution = new Normal();
        private readonly MersenneTwister _random = new MersenneTwister();

        // The points inside the sphere are uniformly distributed
        // For explanation look at: http://math.stackexchange.com/a/87238
        public IList<Point> Generate(int dimensions, double radius, double[] center, int positives,
            int negatives = 0)
        {
            var totalPointsCount = positives + negatives;

            var result = new List<Point>(totalPointsCount);

            for (var i = 0; i < totalPointsCount; i++)
            {
                var isPositive = i < positives;
                var randomCoordinates = _normalDistribution.Samples().Take(dimensions).ToArray();
                var length = Math.Sqrt(randomCoordinates.Select(c => Math.Pow(c, 2)).Sum());
                var u = _random.NextDouble();
                var coordinates =
                    randomCoordinates.Select(c => radius*Math.Pow(u, (isPositive ? 1.0 : -1.0) /dimensions)*c/length).ToArray();
                var pointCoordinates =
                    coordinates.Zip(center, (first, second) => first + second).ToArray();

                result.Add(new Point(pointCoordinates) {Label = isPositive});
            }

            return result;
        }
    }
}