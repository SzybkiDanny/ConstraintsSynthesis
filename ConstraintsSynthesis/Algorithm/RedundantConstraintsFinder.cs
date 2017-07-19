using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using ConstraintsSynthesis.Model;
using MathNet.Numerics.Random;

namespace ConstraintsSynthesis.Algorithm
{
    public class RedundantConstraintsFinder
    {
        private readonly Solution _solution;
        private readonly MersenneTwister _random = new MersenneTwister(Program.Seed);
        private List<Point> _randomPoints;

        public RedundantConstraintsFinder(Solution solution)
        {
            _solution = solution;
        }

        public RedundantConstraintsFinder(Solution solution, int randomPointsCount, double marginExpansion) : this(solution)
        {
            GenerateNewRandomPoints(randomPointsCount, marginExpansion);
        }

        public void GenerateNewRandomPoints(int randomPointsCount = 10000, double marginExpansion = 0.5)
        {
            _randomPoints = new List<Point>(randomPointsCount);

            var ranges = _solution.Cluster.Maximums.Zip(_solution.Cluster.Minimums,
                (max, min) => max - min).ToArray();
            var minimums =
                _solution.Cluster.Minimums.Select((m, i) => m - ranges[i] * marginExpansion).ToArray();
            var maximums =
                _solution.Cluster.Maximums.Select((m, i) => m + ranges[i] * marginExpansion).ToArray();
            var dimensions = _solution.Cluster.Dimensions;

            for (var i = 0; i < randomPointsCount; i++)
            {
                var pointCoordinates = _random.NextDoubles(dimensions);

                for (var j = 0; j < dimensions; j++)
                {
                    pointCoordinates[j] = minimums[j] +
                                          pointCoordinates[j] * (maximums[j] - minimums[j]);
                }

                _randomPoints.Add(new Point(pointCoordinates));
            }
        }

        public List<int> FindRedundantConstraints(int pointsBetweenConstraintsMaximum = 5, double angleSimilarityMarigin = 5.0)
        {
            var constraints = _solution.Constraints.Cast<Constraint>().ToList();
            var similarConstraints = FindSimilarConstraints(constraints, angleSimilarityMarigin);
            var redundant = new List<int>();
            var satisfaction = new ConstraintsSatisfaction(constraints, _randomPoints);
            satisfaction.CalculateMargins();

            foreach (var firstConstraint in similarConstraints)
            {
                var setUnsatisfiedByFirst =
                    satisfaction.GetNotSatisfyingPointsIndices(constraints[firstConstraint.Key])
                        .ToList();

                foreach (var secondConstraintIndex in firstConstraint.Value)
                {
                    var setUnsatisfiedBySecond =
                        satisfaction.GetNotSatisfyingPointsIndices(constraints[secondConstraintIndex])
                            .ToList();

                    if (setUnsatisfiedByFirst.Except(setUnsatisfiedBySecond).Count()
                            <= pointsBetweenConstraintsMaximum)
                        redundant.Add(firstConstraint.Key);
                    else if (setUnsatisfiedBySecond.Except(setUnsatisfiedByFirst).Count()
                                <= pointsBetweenConstraintsMaximum)
                        redundant.Add(secondConstraintIndex);
                }
            }

            return redundant.Distinct().ToList();
        }

        private IDictionary<int, List<int>> FindSimilarConstraints(IList<Constraint> constraints, double angleSimilarityMarigin)
        {
            var similarConstraints = new Dictionary<int, List<int>>();

            for (var i = 0; i < constraints.Count; i++)
            {
                similarConstraints[i] = new List<int>();

                var constraintCoefficients = constraints[i].Terms.Values.ToArray();

                for (var j = 0; j < constraints.Count; j++)
                {
                    if (j == i)
                        continue;

                    var secondConstraintCoefficients = constraints[j].Terms.Values.ToArray();
                    var cosinus = constraintCoefficients.Dot(secondConstraintCoefficients) /
                                  (constraintCoefficients.Euclidean() *
                                   secondConstraintCoefficients.Euclidean());

                    cosinus = cosinus > 1 ? 1 : cosinus < -1 ? -1 : cosinus;

                    var angle = Math.Acos(cosinus) * 180 / Math.PI;

                    if (angle <= angleSimilarityMarigin)
                        similarConstraints[i].Add(j);
                }
            }

            return similarConstraints;
        }
    }
}
