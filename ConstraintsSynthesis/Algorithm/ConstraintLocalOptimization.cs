using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.Random;
using MethodTimer;

namespace ConstraintsSynthesis.Algorithm
{
    public class ConstraintLocalOptimization
    {
        private static readonly MersenneTwister Random = new MersenneTwister(Program.Seed);
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }
        public double MinimalCoefficientOptimizationStep = 0.15;

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
                Constraint.InvertInequalitySign();

            return this;
        }


        private double CalculateNewCoefficientValue(int stepSign, double step, double oldValue)
        {
            if (stepSign > 0)
                return oldValue * step;

            return oldValue / step;
        }

        [Time("Optimizing coefficients")]
        public ConstraintLocalOptimization OptimizeCoefficients()
        {
            var stepSign = Constraint.Sign == Inequality.GreaterThanOrEqual ? 1 : -1;
            var signChanged = false;
            var step = CalculateCoefficientOptimizationStep();

            while (NotSatisfiedPoints.Count > 0)
            {
                Term selectedTerm = null;
                var leastNotSatisfiedPoints = NotSatisfiedPoints.Count;

                foreach (var term in Constraint.Terms.Keys.ToList())
                {
                    var notSatisfiedPoints = TestCoefficientChange(term, step, stepSign);

                    if (notSatisfiedPoints >= leastNotSatisfiedPoints)
                        continue;

                    selectedTerm = term;
                    leastNotSatisfiedPoints = notSatisfiedPoints;
                }

                if (selectedTerm != null)
                {
                    Constraint[selectedTerm] = CalculateNewCoefficientValue(stepSign, step, Constraint[selectedTerm]);
                    signChanged = false;
                }
                else if (NotSatisfiedPoints.Count > 0 && !signChanged)
                {
                    stepSign *= -1;
                    signChanged = true;
                }
                else if (NotSatisfiedPoints.Count > 0 && signChanged)
                {
                    Constraint.AbsoluteTerm += step * (Constraint.Sign == Inequality.GreaterThanOrEqual ? -1 : 1);
                    signChanged = false;
                }
                else
                {
                    step *= 2;
                    signChanged = false;
                }
            }

            return this;
        }

        [Time("Squeezing constraints")]
        public ConstraintLocalOptimization SqueezeConstraint()
        {
            var termsToSqueeze = new List<Term>(Constraint.Terms.Keys);
            var stepSign = Constraint.Sign == Inequality.LessThanOrEqual ? 1 : -1;

            Constraint.AbsoluteTerm += 5 * stepSign;

            while (termsToSqueeze.Count > 0)
            {
                var selectedIndex = Random.Next(termsToSqueeze.Count);
                var selectedTerm = termsToSqueeze[selectedIndex];
                var testResult = TestCoefficientChange(selectedTerm,
                    MinimalCoefficientOptimizationStep, stepSign);

                if (testResult == 0)
                {
                    Constraint[selectedTerm] += MinimalCoefficientOptimizationStep * stepSign;
                }
                else
                {
                    termsToSqueeze.RemoveAt(selectedIndex);
                }
            }

            return this;
        }

        private int TestCoefficientChange(Term term, double step, int stepSign)
        {
            if (term == null)
                return NotSatisfiedPoints.Count;

            Constraint[term] = CalculateNewCoefficientValue(stepSign, step, Constraint[term]);
            var result = NotSatisfiedPoints.Count;
            Constraint[term] = CalculateNewCoefficientValue(-stepSign, step, Constraint[term]);

            return result;
        }

        private double CalculateCoefficientOptimizationStep() =>
            1 + Math.Max((double)NotSatisfiedPoints.Count / PositivePoints.Count, MinimalCoefficientOptimizationStep);
    }
}
