using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;
using MethodTimer;

namespace ConstraintsSynthesis.Algorithm
{
    public class ConstraintLocalOptimization
    {
        private readonly MersenneTwister _random = new MersenneTwister(Program.Seed);
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }
        public const double CoefficientOptimizationStep = 0.005;

        public int SatisfiedPointsCount =>
            PositivePoints.Count(Constraint.IsSatisfying);

        public ConstraintLocalOptimization(Constraint constraint, Cluster cluster)
        {
            Constraint = constraint;
            Points = cluster.Points;
            PositivePoints = Points.Where(p => p.Label).ToList();
        }

        [Time("Optimizing sign")]
        public ConstraintLocalOptimization OptimizeSign()
        {
            if (SatisfiedPointsCount * 2 < PositivePoints.Count)
                Constraint.InvertInequalitySing();

            return this;
        }

        [Time("Optimizing coefficients")]
        public ConstraintLocalOptimization OptimizeCoefficients()
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

            return this;
        }

        [Time("Squeezing constraints")]
        public ConstraintLocalOptimization SqueezeConstraint()
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

            return this;
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
