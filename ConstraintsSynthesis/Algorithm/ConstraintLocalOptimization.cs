using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;

namespace ConstraintsSynthesis.Algorithm
{
    public class ConstraintLocalOptimization
    {
        private readonly MersenneTwister _random = new MersenneTwister();
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }
        public const double CoefficientOptimizationStep = 0.005;

        public int SatisfiedPointsCount =>
            PositivePoints.Count(Constraint.IsSatisfying);

        public ConstraintLocalOptimization(Constraint constraint, List<Point> points)
        {
            Constraint = constraint;
            Points = points;
            PositivePoints = Points.Where(p => p.Label).ToList();
        }

        public void Optimize(bool optimizeSign = true, bool optimizeCoefficients = true, bool squeezeConstraint = true)
        {
            if (optimizeSign && SatisfiedPointsCount*2 < PositivePoints.Count)
                Constraint.InvertInequalitySing();

            if (optimizeCoefficients)
                OptimizeCoefficients();

            if (squeezeConstraint)
                SqueezeConstraint();

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

        private void SqueezeConstraint()
        {
            var termsToSqueeze = new List<Term>(Constraint.Terms.Keys);
            var stepSign = Constraint.Sign == Inequality.LessThanOrEqual ? 1 : -1;

            Constraint.AbsoluteTerm += 20 * stepSign;

            while (termsToSqueeze.Count > 0)
            {
                var selectedIndex = _random.Next(termsToSqueeze.Count);
                var selectedTerm = termsToSqueeze[selectedIndex];
                var testResult = TestCoefficientChange(selectedTerm,
                    CoefficientOptimizationStep*stepSign);

                if (testResult == 0)
                {
                    Constraint[selectedTerm] += CoefficientOptimizationStep*stepSign;
                }
                else
                {
                    termsToSqueeze.RemoveAt(selectedIndex);
                }
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
