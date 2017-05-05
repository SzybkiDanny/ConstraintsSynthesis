using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Algorithm;

namespace ConstraintsSynthesis.Model
{
    public class Solution
    {
        public Cluster Cluster { get; }
        public List<LinearConstraint> Constraints { get; } = new List<LinearConstraint>();
        public List<LinearConstraint> InitialConstraints { get; private set; }

        public Solution(Cluster cluster)
        {
            Cluster = cluster;
        }

        public Solution GenerateInitialSolution()
        {
            var initialConstraints = InitialSolutionGenerator.GenerateInitialConstraints(Cluster).ToArray();

            InitialConstraints = new List<LinearConstraint>(initialConstraints);
            Constraints.AddRange(initialConstraints);

            return this;
        }

        public Solution GenerateImprovedInitialConstraints(int iterations = 1)
        {
            if (InitialConstraints == null)
                throw new Exception("No initial constraints are generated");

            for (var i = 0; i < iterations; i++)
            {
                foreach (var initialConstraint in InitialConstraints)
                {
                    var newConstraint = initialConstraint.Clone() as LinearConstraint;
                    var optimizer = new ConstraintLocalOptimization(newConstraint, Cluster);

                    optimizer.SqueezeConstraint();
                    Constraints.Add(optimizer.Constraint as LinearConstraint);
                }
            }

            return this;
        }

        public Solution GenerateImprovingConstraints(int count = 100)
        {
            var randomLinearConstraints = LinearConstraintsGenerator.GenerateRandomLinearConstraints(Cluster, count);

            foreach (var constraint in randomLinearConstraints)
            {
                var optimizer = new ConstraintLocalOptimization(constraint, Cluster);

                optimizer.OptimizeSign()
                    .OptimizeCoefficients()
                    .SqueezeConstraint();
                Constraints.Add(optimizer.Constraint as LinearConstraint);
            }

            return this;
        }

        public Solution RemoveRedundantConstraints(int samplingSize = 100)
        {
            var redundantConstraintsFinder = new RedundantConstraintsFinder(this, samplingSize, 0.0);
            var redundantIndices = redundantConstraintsFinder.FindRedundantConstraints();

            redundantIndices.Sort();
            redundantIndices.Reverse();

            foreach (var indexToRemove in redundantIndices)
            {
                Constraints.RemoveAt(indexToRemove);
            }

            return this;
        }
    }
}
