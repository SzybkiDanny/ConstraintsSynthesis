using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstraintsSynthesis.Algorithm;
using ConstraintsSynthesis.Model.Enums;
using MethodTimer;
using ConstraintMetric = ConstraintsSynthesis.Model.Enums.ConstraintMetric;

namespace ConstraintsSynthesis.Model
{
    public class Solution
    {
        private readonly List<LinearConstraint> _constraints = new List<LinearConstraint>();
        private readonly List<LinearConstraint> _initialConstraints = new List<LinearConstraint>();

        public Cluster Cluster { get; }
        public IEnumerable<LinearConstraint> Constraints =>
            _constraints.Select(c => c.Translate(Cluster.Means));
        public IEnumerable<LinearConstraint> InitialConstraints =>
            _initialConstraints.Select(c => c.Translate(Cluster.Means));
        public int Index { get; set; }

        public Solution(Cluster cluster)
        {
            Cluster = cluster;
        }

        [Time("Generating initial constraints")]
        public Solution GenerateInitialSolution()
        {
            var initialConstraints = InitialSolutionGenerator.GenerateInitialConstraints(Cluster.GetCentralizedCluster()).ToList();

            _initialConstraints.AddRange(initialConstraints);
            _constraints.AddRange(initialConstraints);

            return this;
        }

        [Time("Generating constraints based on initial constraints")]
        public Solution GenerateImprovedInitialConstraints(int iterations = 1)
        {
            if (_initialConstraints == null)
                throw new Exception("No initial constraints are generated");

            for (var i = 0; i < iterations; i++)
            {
                foreach (var initialConstraint in _initialConstraints)
                {
                    var newConstraint = initialConstraint.Clone() as LinearConstraint;
                    var optimizer = new ConstraintLocalOptimization(newConstraint, Cluster.GetCentralizedCluster());

                    optimizer.SqueezeConstraint();
                    _constraints.Add(optimizer.Constraint as LinearConstraint);
                }
            }

            return this;
        }

        [Time("Generating random constraints")]
        public Solution GenerateRandomConstraints(
            ConstraintsGeneration constraintsGeneration = ConstraintsGeneration.CrossingRandomPoint, int count = 100,
            bool optimizeSign = true, bool optimizeCoefficients = true)
        {
            Func<Cluster, int, IEnumerable<LinearConstraint>> generationMethod = null;

            switch (constraintsGeneration)
            {
                case ConstraintsGeneration.CrossingRandomPoint:
                    generationMethod = LinearConstraintsGenerator.GenerateRandomLinearConstraints;
                    break;
                case ConstraintsGeneration.CrossingRandomPointAndOrigin:
                    generationMethod = LinearConstraintsGenerator.GenerateRandomLinearConstraintsCrossingOrigin;
                    break;
            }

            var randomLinearConstraints = generationMethod(Cluster.GetCentralizedCluster(), count);

            foreach (var constraint in randomLinearConstraints)
            {
                var optimizer = new ConstraintLocalOptimization(constraint, Cluster.GetCentralizedCluster());

                if (optimizeSign)
                    optimizer.OptimizeSign();

                if (optimizeCoefficients)
                    optimizer.OptimizeCoefficients();

                _constraints.Add(optimizer.Constraint as LinearConstraint);
            }

            return this;
        }

        [Time("Removing redundant constraints")]
        public Solution RemoveRedundantConstraints(ConstraintMetric constraintMetric = ConstraintMetric.MostUnsatisfied, int samplingSize = 100,
            double marginExpansion = 0.5, double angleSimilarityMarigin = 5.0, bool reduceSatisfiedPoints = true)
        {
            var randomPoints = Cluster.GenerateRandomPointsAroundCluster(samplingSize, marginExpansion);
            Func<Constraint, double> constraintUtilityFunction = null;
            var constraints = Constraints.Cast<Constraint>().ToList();


            switch (constraintMetric)
            {
                case ConstraintMetric.DistanceFromCentroid:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.DistanceFromCentroid(c, new Point(Cluster.Centroid));
                    break;
                case ConstraintMetric.DistanceFromMeans:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.DistanceFromMeans(c, new Point(Cluster.Means));
                    break;
                case ConstraintMetric.DistanceFromSatisfied:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.DistanceFromSatisfied(c, randomPoints);
                    break;
                case ConstraintMetric.DistanceFromUnsatisfied:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.DistanceFromUnsatisfied(c, randomPoints);
                    break;
                case ConstraintMetric.AvgDistanceFromSatisfied:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.AvgDistanceFromSatisfied(c, randomPoints);
                    break;
                case ConstraintMetric.AvgDistanceFromUnsatisfied:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.AvgDistanceFromUnsatisfied(c, randomPoints);
                    break;
                case ConstraintMetric.MostUnsatisfied:
                    constraintUtilityFunction =
                        c => Algorithm.ConstraintMetric.MostUnsatisfied(c, randomPoints);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(constraintMetric), constraintMetric, null);
            }

            var redundantIndices =
                RedundantConstraintsFinder.FindRedundantConstraints(constraintUtilityFunction, constraints, randomPoints,
                    angleSimilarityMarigin, reduceSatisfiedPoints).ToList();

            redundantIndices.Sort();
            redundantIndices.Reverse();

            foreach (var indexToRemove in redundantIndices)
            {
                _constraints[indexToRemove].IsMarkedRedundant = true;
            }

            return this;
        }

        [Time("Generating readable model")]
        public Solution GenerateReadableSolution(out string[] readableSolution)
        {
            var result = new List<string>();

            foreach (var constraint in Constraints.Where(c => !c.IsMarkedRedundant))
            {
                constraint.ConvertToLessThanOrEqual();
                result.Add($"{constraint} + (1 - b{Index}) * M");
            }

            readableSolution = result.ToArray();

            return this;
        }

        [Time("Generating negative points from cluster's points distribution")]
        public Solution GenerateNegativePointsFromClusterDistribution(double quantile = 0.999, int count = 1000)
        {
            var negativePoints = Cluster.GenerateNegativePointsFromPositivesDistribution(quantile, count);

            Cluster.Points.AddRange(negativePoints);
            
            return this;
        }
    }
}
