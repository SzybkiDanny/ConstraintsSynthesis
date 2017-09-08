using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;
using MethodTimer;

namespace ConstraintsSynthesis.Algorithm
{
    public class ConstraintLocalOptimization
    {
        private const int ProbingCoefficient = 10000;
        private static readonly MersenneTwister Random = new MersenneTwister(Program.Seed);
        private List<Point> PositivePoints { get; }
        private List<Point> NotSatisfiedPoints =>
            PositivePoints.Where(p => !Constraint.IsSatisfying(p)).ToList();

        private Dictionary<Term, ChangeDirection> _coefficientsOptimizationDirection = new Dictionary<Term, ChangeDirection>();
        private Dictionary<Term, double> _coefficientsStepFactor = new Dictionary<Term, double>();

        public Constraint Constraint { get; }
        public List<Point> Points { get; }

        public int SatisfiedPointsCount =>
            PositivePoints.Count(Constraint.IsSatisfying);

        public ConstraintLocalOptimization(Constraint constraint, Cluster cluster)
        {
            Constraint = constraint;
            Points = cluster.Points;
            PositivePoints = Points.Where(p => p.Label).ToList();
            ProbeCoefficientChangeDirection();
        }

        [Time("Optimizing sign")]
        public ConstraintLocalOptimization OptimizeSign()
        {
            if (SatisfiedPointsCount * 2 < PositivePoints.Count)
                Constraint.InvertInequalitySign();

            return this;
        }

        [Time("Optimizing coefficients")]
        public ConstraintLocalOptimization OptimizeCoefficients()
        {
            var terms = Constraint.Terms.Keys.ToList();
            var notSatisfied = NotSatisfiedPoints.Count;

            while (notSatisfied > 0)
            {
                var coefficientAvg = Constraint.Terms.Values.Average(c => Math.Abs(c));
                var step = CalculateCoefficientOptimizationStep();

                foreach (var term in terms)
                {
                    Constraint[term] = CalculateNewCoefficientValue((int)_coefficientsOptimizationDirection[term],
                        _coefficientsStepFactor[term] * Math.Abs(Constraint[term]) / coefficientAvg, Constraint[term]);
                }

                Constraint.AbsoluteTerm += step * coefficientAvg * (Constraint.Sign == Inequality.GreaterThanOrEqual ? -1 : 1);
                ProbeCoefficientChangeDirection(step);
                notSatisfied = NotSatisfiedPoints.Count;

                var notSatisfiedAfterChange = NotSatisfiedPoints.Count;

                if (notSatisfiedAfterChange == 0)
                    return this;

                if (notSatisfiedAfterChange < notSatisfied)
                    notSatisfied = notSatisfiedAfterChange;
            }

            return this;
        }

        private int TestCoefficientChange(Term term, double step, int stepSign)
        {
            var oldValue = Constraint[term];
            Constraint[term] = CalculateNewCoefficientValue(stepSign, step, Constraint[term]);
            var result = NotSatisfiedPoints.Count;
            Constraint[term] = oldValue;
            return result;
        }

        private double CalculateCoefficientOptimizationStep() =>
            1 + (double)NotSatisfiedPoints.Count / PositivePoints.Count;

        private double CalculateNewCoefficientValue(int stepSign, double step, double oldValue)
        {
            if (stepSign > 0)
                return oldValue + step;

            return oldValue - step;
        }

        private void ProbeCoefficientChangeDirection(double step = ProbingCoefficient)
        {
            var notSatisfiedCount = NotSatisfiedPoints.Count;

            foreach (var term in Constraint.Terms.Keys.ToList())
            {
                if (!_coefficientsOptimizationDirection.ContainsKey(term))
                    _coefficientsOptimizationDirection[term] = ChangeDirection.Decreasing;

                if (!_coefficientsStepFactor.ContainsKey(term))
                    _coefficientsStepFactor[term] = 1.0;

                var noCoefficientChange = TestCoefficientChange(term, step, (int)_coefficientsOptimizationDirection[term]);

                if (noCoefficientChange < notSatisfiedCount)
                {
                    _coefficientsStepFactor[term] = 1 + noCoefficientChange / notSatisfiedCount;
                    continue;
                }

                _coefficientsOptimizationDirection[term] = (ChangeDirection)((int)_coefficientsOptimizationDirection[term] * -1);

                var coefficientChange = TestCoefficientChange(term, step, (int)_coefficientsOptimizationDirection[term]);

                if (coefficientChange < notSatisfiedCount)
                {
                    _coefficientsStepFactor[term] = 1 + coefficientChange / notSatisfiedCount;
                    continue;
                }

                if (coefficientChange > noCoefficientChange)
                    _coefficientsOptimizationDirection[term] = (ChangeDirection)((int)_coefficientsOptimizationDirection[term] * -1);

                _coefficientsStepFactor[term] = 1;

            }
        }

        enum ChangeDirection
        {
            Decreasing = -1,
            Increasing = 1,
        }
    }
}
