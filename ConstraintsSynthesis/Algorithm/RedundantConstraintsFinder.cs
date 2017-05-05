using System.Collections.Generic;
using System.Linq;
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

        public List<int> FindRedundantConstraints()
        {
            var constraints = _solution.Constraints.Cast<Constraint>().ToList();
            var redundant = new List<int>();

            var cs = new ConstraintsSatisfaction(constraints, _randomPoints);

            for (var i = 0; i < constraints.Count; i++)
            {
                for (var j = i + 1; j < constraints.Count; j++)
                {
                    var firstUnsatisfiedSet = cs.GetUnsatisfyingPointsIndices(constraints[i]).ToList();
                    var secondUnsatisfiedSet = cs.GetUnsatisfyingPointsIndices(constraints[j]).ToList();

                    if (!firstUnsatisfiedSet.Except(secondUnsatisfiedSet).Any())
                        redundant.Add(i);
                    else if (!secondUnsatisfiedSet.Except(firstUnsatisfiedSet).Any())
                        redundant.Add(j);
                }
            }

            return redundant.Distinct().ToList();
        }
    }
}
