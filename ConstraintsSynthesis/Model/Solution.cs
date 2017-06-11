using System;
using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Algorithm;
using MethodTimer;

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

        [Time("Generating initial constraints")]
        public Solution GenerateInitialSolution()
        {
            var initialConstraints = InitialSolutionGenerator.GenerateInitialConstraints(Cluster).ToArray();

            InitialConstraints = new List<LinearConstraint>(initialConstraints);
            Constraints.AddRange(initialConstraints);

            return this;
        }

        [Time("Generating constraints based on initial constraints")]
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
        [Time("Generating random constraints")]
        public Solution GenerateRandomConstraints(int count = 100, bool optimizeSign = true, bool optimizeCoefficients = true, bool squeezeConstraints = true)
        {
            var randomLinearConstraints = LinearConstraintsGenerator.GenerateRandomLinearConstraints(Cluster, count);

            foreach (var constraint in randomLinearConstraints)
            {
                var optimizer = new ConstraintLocalOptimization(constraint, Cluster);

                if (optimizeSign)
                    optimizer.OptimizeSign();
                
                if (optimizeCoefficients)
                    optimizer.OptimizeCoefficients();

                if (squeezeConstraints)
                    optimizer.SqueezeConstraint();

                Constraints.Add(optimizer.Constraint as LinearConstraint);
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
                Constraints.RemoveAt(indexToRemove);
            }

            return this;
        }
    }
}
