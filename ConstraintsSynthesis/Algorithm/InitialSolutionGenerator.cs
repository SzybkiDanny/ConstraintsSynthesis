using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public static class InitialSolutionGenerator
    {
        public static IEnumerable<LinearConstraint> GenerateInitialConstraints(IList<Point> data)
        {
            var dimensions = data.First().Coordinates.Length;

            for (var i = 0; i < dimensions; i++)
            {
                var min = data.Min(p => p[i]);
                var minConstraint = new LinearConstraint(new Dictionary<int, double>() { {i, 1.0} }, dimensions, min)
                {
                    Sign = Inequality.GreaterThanOrEqual
                };

                yield return minConstraint;

                var max = data.Max(p => p[i]);
                var maxConstraint = new LinearConstraint(new Dictionary<int, double>() { {i, 1.0} }, dimensions, max);

                yield return maxConstraint;
            }
        }
    }
}