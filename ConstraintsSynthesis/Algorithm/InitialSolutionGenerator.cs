using System.Collections.Generic;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public static class InitialSolutionGenerator
    {
        public static IEnumerable<LinearConstraint> GenerateInitialConstraints(Cluster cluster)
        {
            for (var i = 0; i < cluster.Dimensions; i++)
            {
                var minConstraint = new LinearConstraint(new Dictionary<int, double>()
                    { {i, 1.0} }, cluster.Dimensions, cluster.Minimums[i])
                {
                    Sign = Inequality.GreaterThanOrEqual
                };

                yield return minConstraint;

                var maxConstraint = new LinearConstraint(new Dictionary<int, double>()
                    { {i, 1.0} }, cluster.Dimensions, cluster.Maximums[i]);

                yield return maxConstraint;
            }
        }
    }
}