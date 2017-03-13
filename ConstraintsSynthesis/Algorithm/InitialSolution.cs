using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    internal class InitialSolution
    {
        public IList<Constraint> GenerateInitialConstraints(IList<Point> data)
        {
            var constraints = new List<Constraint>();
            var dimensions = data.First().Coordinates.Length;

            for (var i = 0; i < dimensions; i++)
            {
                var min = data.Min(p => p[i]);
                var minConstraint = new Constraint
                {
                    AbsoluteTerm = min,
                    [new Term() {[i] = 1.0}] = 1.0,
                    Sign = Inequality.GreaterThanOrEqual
                };
                constraints.Add(minConstraint);

                var max = data.Max(p => p[i]);
                var maxConstraint = new Constraint
                {
                    AbsoluteTerm = max,
                    [new Term {[i] = 1.0}] = 1.0
                };
                constraints.Add(maxConstraint);
            }

            return constraints;
        }
    }
}