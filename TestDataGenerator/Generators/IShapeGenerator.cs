using System.Collections.Generic;
using TestDataGenerator.Model;

namespace TestDataGenerator.Generators
{
    internal interface IShapeGenerator
    {
        IList<Point> Generate(int dimensions, double radius, double[] center, int positives, int negatives = 0);
    }
}