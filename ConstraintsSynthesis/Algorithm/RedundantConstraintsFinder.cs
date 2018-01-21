using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public static class RedundantConstraintsFinder
    {
        public static HashSet<int> FindRedundantConstraints(Func<Constraint, double> constraintUtilityMetric, List<Constraint> constraints, List<Point> randomPoints, 
            double angleSimilarityMarigin = 5.0, bool reduceSatisfiedPoints = true)
        {
            var redundantConstraints = new HashSet<int>();
            var similarConstraints = FindSimilarConstraints(constraints, angleSimilarityMarigin);
            var constraintsSatisfaction = new ConstraintsSatisfaction(constraints, randomPoints);
            constraintsSatisfaction.CalculateMargins(reduceSatisfiedPoints);

            var allUnsatisfiedPoints = new HashSet<int>();

            foreach (var firstConstraint in similarConstraints)
            {
                var unsatisfiedPoints = constraintsSatisfaction.GetNotSatisfyingPointsIndices(firstConstraint.Key);

                if (allUnsatisfiedPoints.IsSupersetOf(unsatisfiedPoints))
                {
                    redundantConstraints.Add(firstConstraint.Key);
                    continue;
                }

                var bestConstraint = firstConstraint.Key;
                var distanctFromBest = constraintUtilityMetric(constraints[bestConstraint]);

                foreach (var secondConstraintIndex in firstConstraint.Value)
                {
                    if (secondConstraintIndex > firstConstraint.Key)
                        continue;

                    var distance = constraintUtilityMetric(constraints[secondConstraintIndex]);

                    if (distance < distanctFromBest)
                    {
                        redundantConstraints.Add(bestConstraint);
                        bestConstraint = secondConstraintIndex;
                    }
                    else
                    {
                        redundantConstraints.Add(secondConstraintIndex);
                    }
                }

                allUnsatisfiedPoints.UnionWith(constraintsSatisfaction.GetNotSatisfyingPointsIndices(bestConstraint));
                redundantConstraints.Remove(bestConstraint);
            }

            return redundantConstraints;
        }

        private static IDictionary<int, List<int>> FindSimilarConstraints(IList<Constraint> constraints, double angleSimilarityMarigin)
        {
            var similarConstraints = new Dictionary<int, List<int>>();

            for (var i = 0; i < constraints.Count; i++)
            {
                similarConstraints[i] = new List<int>();

                var constraintCoefficients = constraints[i].Terms.Values.ToArray();

                for (var j = 0; j < constraints.Count; j++)
                {
                    if (j == i || constraints[i].Sign != constraints[j].Sign)
                        continue;

                    var secondConstraintCoefficients = constraints[j].Terms.Values.ToArray();
                    var cosinus = constraintCoefficients.Dot(secondConstraintCoefficients) /
                                  (constraintCoefficients.Euclidean() *
                                   secondConstraintCoefficients.Euclidean());

                    cosinus = cosinus > 1 ? 1 : cosinus < -1 ? -1 : cosinus;

                    var angle = Math.Abs(Math.Acos(cosinus) * 180 / Math.PI);

                    if (angle <= angleSimilarityMarigin)
                        similarConstraints[i].Add(j);
                }
            }

            return similarConstraints;
        }
    }
}
