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
            var initialConstraints = InitialSolutionGenerator.GenerateInitialConstraints(Cluster.Points).ToArray();

            InitialConstraints = new List<LinearConstraint>(initialConstraints);
            Constraints.AddRange(initialConstraints);

            return this;
        }

        public Solution GenerateImprovedInitialConstraints()
        {
            if (InitialConstraints == null)
                throw new Exception("No initial constraints are generated");

            foreach (var initialConstraint in InitialConstraints)
            {
                var newConstraint = initialConstraint.Clone() as LinearConstraint;
                var optimizer = new ConstraintLocalOptimization(newConstraint, Cluster.Points);

                optimizer.Optimize(optimizeCoefficients: false, optimizeSign: false, squeezeConstraint: true);
                Constraints.Add(optimizer.Constraint as LinearConstraint);
            }

            return this;
        }

        public Solution GenerateImprovingConstraints(int count = 100)
        {
            var randomLinearConstraints = LinearConstraintsGenerator.GenerateRandomLinearConstraints(Cluster.Points, count);

            foreach (var constraint in randomLinearConstraints)
            {
                var optimizer = new ConstraintLocalOptimization(constraint, Cluster.Points);

                optimizer.Optimize(optimizeCoefficients: true, optimizeSign: false, squeezeConstraint: true);
                Constraints.Add(optimizer.Constraint as LinearConstraint);
            }

            return this;
        }
    }
}
