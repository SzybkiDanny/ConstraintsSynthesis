using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;
using TestDataGenerator.Model;

namespace TestDataGenerator.Generators
{
    internal class MultipleCloudsGenerator : IShapeGenerator
    {
        private readonly MersenneTwister _random = new MersenneTwister();

        public IList<Point> Generate(int dimensions, double radius, double[] center, int positives,
            int negatives = 0, int multiplicity = 1)
        {
            var totalPointsCount = positives + negatives;
            var positivesGenerated = 0;
            var negativesGenerated = 0;
            var centers = new double[multiplicity][];

            for (var i = 0; i < multiplicity; i++)
            {
                centers[i] = _random.NextDoubles(dimensions).Select(c => (c - 0.5) * 2 * radius / Math.Sqrt(dimensions)).ToArray();
            }

            var result = new List<Point>(totalPointsCount);

            while (positivesGenerated < positives || negativesGenerated < negatives)
            {
                var randomCoordinates = _random.NextDoubles(dimensions).Select(r => (r - 0.5) * radius).ToArray();
                var length = Math.Sqrt(randomCoordinates.Select(c => Math.Pow(c, 2)).Sum());
                var isPositive = length <= radius;
                var randomCenter = centers[_random.Next(multiplicity)];
                var pointCoordinates = randomCoordinates
                    .Select(v => v * _random.NextDouble() / length)
                    .Zip(center, (first, second) => first + second)
                    .Zip(randomCenter, (first, second) => first + second)
                    .ToArray();

                if (isPositive && positivesGenerated < positives)
                {
                    result.Add(new Point(pointCoordinates) { Label = isPositive });
                    positivesGenerated++;
                }
                else if (!isPositive && negativesGenerated < negatives)
                {
                    result.Add(new Point(pointCoordinates) { Label = isPositive });
                    negativesGenerated++;
                }
            }

            return result;
        }
    }
}