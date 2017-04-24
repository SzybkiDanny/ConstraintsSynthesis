using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Random;
using TestDataGenerator.Model;

namespace TestDataGenerator.Generators
{
    internal class SphereFromUniformGenerator : IShapeGenerator
    {
        private readonly MersenneTwister _random = new MersenneTwister();

        public IList<Point> Generate(int dimensions, double radius, double[] center, int positives,
            int negatives = 0)
        {
            var totalPointsCount = positives + negatives;
            var positivesGenerated = 0;
            var negativesGenerated = 0;

            var result = new List<Point>(totalPointsCount);

            while (positivesGenerated < positives || negativesGenerated < negatives)
            {
                var randomCoordinates = _random.NextDoubles(dimensions).Select(r => (r - 0.5) * radius * 2 * Constants.Sqrt2).ToArray();
                var length = Math.Sqrt(randomCoordinates.Select(c => Math.Pow(c, 2)).Sum());
                var isPositive = length <= radius;
                var pointCoordinates =
                    randomCoordinates.Zip(center, (first, second) => first + second).ToArray();

                if (isPositive && positivesGenerated < positives)
                {
                    result.Add(new Point(pointCoordinates) {Label = isPositive});
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