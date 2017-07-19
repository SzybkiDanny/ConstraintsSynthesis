using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class ConstraintsSatisfaction
    {
        public List<Constraint> Constraints { get; set; }
        public List<Point> Points { get; set; }
        public List<List<double>> Margins { get; private set; }

        public ConstraintsSatisfaction()
        {   
        }

        public ConstraintsSatisfaction(List<Constraint> constraints, List<Point> points)
        {
            Constraints = constraints;
            Points = points;
        }

        public void CalculateMargins(bool reducePoints = true)
        {
            var margins = new List<List<double>>(Points.Count);

            for (var i = 0; i < Points.Count; i++)
            {
                var pointMargins = new List<double>(Constraints.Count);
                var pointSatisfiesAll = true;
                var pointSatisfiesNone = true;

                for (var j = 0; j < Constraints.Count; j++)
                {
                    var margin = Constraints[j].MarginForPoint(Points[i]);

                    pointMargins.Add(margin);

                    if (margin < 0)
                        pointSatisfiesAll = false;
                    else
                        pointSatisfiesNone = false;
                }

                if (reducePoints && (pointSatisfiesAll || pointSatisfiesNone))
                    Points.RemoveAt(i--);
                else
                    margins.Add(pointMargins);
            }

            Margins = margins;
        }

        public IEnumerable<int> GetSatisfyingPointsIndices(Constraint constraint)
        {
            var constraintIndex = Constraints.IndexOf(constraint);

            return GetSatisfyingPointsIndices(constraintIndex);
        }

        public IEnumerable<int> GetNotSatisfyingPointsIndices(Constraint constraint)
        {
            var constraintIndex = Constraints.IndexOf(constraint);

            return GetNotSatisfyingPointsIndices(constraintIndex);
        }

        public IEnumerable<int> GetSatisfyingPointsIndices(int constraintIndex) =>
            GetPointsSatisfyingCondition(constraintIndex, m => m >= 0);

        public IEnumerable<int> GetNotSatisfyingPointsIndices(int constraintIndex) =>
            GetPointsSatisfyingCondition(constraintIndex, m => m < 0);

        private IEnumerable<int> GetPointsSatisfyingCondition(int constraintIndex,
            Predicate<double> condition) =>
                Margins.Select((pointMargins, index) => new { pointIndex = index, margin = pointMargins[constraintIndex]})
                    .Where(c => condition(c.margin)).Select(o => o.pointIndex);
    }
}
