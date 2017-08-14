using System.Collections.Generic;
using ConstraintsSynthesis.Model;

namespace ConstraintsSynthesis.Algorithm
{
    public static class ConstraintMetric
    {
        public static double DistanceFromCentroid(Constraint constraint, Point centroid)
        {
            return constraint.DistanceFromPoint(centroid);
        }

        public static double DistanceFromMeans(Constraint constraint, Point means)
        {
            return constraint.DistanceFromPoint(means);
        }

        public static double DistanceFromUnsatisfied(Constraint constraint, List<Point> randomPoints)
        {
            var distance = 0.0;

            foreach (var randomPoint in randomPoints)
            {
                if (!constraint.IsSatisfying(randomPoint))
                    distance += constraint.DistanceFromPoint(randomPoint);
            }

            return -distance;
        }

        public static double DistanceFromSatisfied(Constraint constraint, List<Point> randomPoints)
        {
            var distance = 0.0;

            foreach (var randomPoint in randomPoints)
            {
                if (constraint.IsSatisfying(randomPoint))
                    distance += constraint.DistanceFromPoint(randomPoint);
            }

            return distance;
        }

        public static double AvgDistanceFromUnsatisfied(Constraint constraint, List<Point> randomPoints)
        {
            var distance = 0.0;
            var count = 0;

            foreach (var randomPoint in randomPoints)
            {
                if (!constraint.IsSatisfying(randomPoint))
                {
                    distance += constraint.DistanceFromPoint(randomPoint);
                    count++;
                }
            }

            return -distance / count;
        }

        public static double AvgDistanceFromSatisfied(Constraint constraint, List<Point> randomPoints)
        {
            var distance = 0.0;
            var count = 0;

            foreach (var randomPoint in randomPoints)
            {
                if (constraint.IsSatisfying(randomPoint))
                {
                    distance += constraint.DistanceFromPoint(randomPoint);
                    count++;
                }
            }

            return distance / count;
        }
    }
}
