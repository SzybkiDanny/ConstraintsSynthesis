using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstraintsSynthesis.Algorithm;
using MethodTimer;

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
        public Solution GenerateRandomConstraints(int count = 100, bool optimizeSign = true, bool optimizeCoefficients = true, bool squeezeConstraints = false)
        {
            var randomLinearConstraints = LinearConstraintsGenerator.GenerateRandomLinearConstraints(Cluster.GetCentralizedCluster(), count);

            foreach (var constraint in randomLinearConstraints)
            {
                var optimizer = new ConstraintLocalOptimization(constraint, Cluster.GetCentralizedCluster());

                if (optimizeSign)
                    optimizer.OptimizeSign();
                
                if (optimizeCoefficients)
                    optimizer.OptimizeCoefficients();

                if (squeezeConstraints)
                    optimizer.SqueezeConstraint();

                _constraints.Add(optimizer.Constraint as LinearConstraint);
            }

            return this;
        }

        [Time("Removing redundant constraints")]
        public Solution RemoveRedundantConstraints(int samplingSize = 100)
        {
            var redundantConstraintsFinder = new RedundantConstraintsFinder(this, samplingSize, 0.0);
            var redundantIndices = redundantConstraintsFinder.FindRedundantConstraints();

            redundantIndices.Sort();
            redundantIndices.Reverse();

            foreach (var indexToRemove in redundantIndices)
            {
                _constraints.RemoveAt(indexToRemove);
            }

            return this;
        }

        [Time("Generating readable model")]
        public StringBuilder GenerateReadableSolution()
        {
            var result = new StringBuilder();

            foreach (var constraint in Constraints)
            {
                constraint.ConvertToLessThanOrEqual();
                result.AppendLine($"{constraint} + (1 - b{Index}) * M");
            }

            return result;
        }
    }
}
