using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public class ConstraintLocalOptimization
    {
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }
        public const double CoefficientOptimizationStep = 0.2;

        public int SatisfiedPointsCount =>
            PositivePoints.Count(Constraint.IsSatisfying);

        public ConstraintLocalOptimization(Constraint constraint, List<Point> points)
        {
            Constraint = constraint;
            Points = points;
            PositivePoints = Points.Where(p => p.Label).ToList();
        }

        public void Optimize(bool optimizeSign = true, bool optimizeCoefficients = true)
        {
            if (optimizeSign && SatisfiedPointsCount*2 < PositivePoints.Count)
                Constraint.InvertInequalitySing();

            if (optimizeCoefficients)
                OptimizeCoefficients();

        }

        private void OptimizeCoefficients()
        {
            while (NotSatisfiedPoints.Count > 0)
            {
                Term selectedTerm = null;
                var leastNotSatisfiedPoints = int.MaxValue;
                var stepSign = Constraint.Sign == Inequality.GreaterThanOrEqual ? 1 : -1;

                foreach (var term in Constraint.Terms.Keys.ToList())
                {
                    var notSatisfiedPoints = TestCoefficientChange(term, CoefficientOptimizationStep*stepSign);

                    if (notSatisfiedPoints >= leastNotSatisfiedPoints)
                        continue;

                    selectedTerm = term;
                    leastNotSatisfiedPoints = notSatisfiedPoints;
                }

                Constraint[selectedTerm] += CoefficientOptimizationStep*stepSign;
            }
        }

        private int TestCoefficientChange(Term term, double step)
        {
            Constraint[term] += step;
            var result = NotSatisfiedPoints.Count;
            Constraint[term] -= step;

            return result;
        }

    }
}
