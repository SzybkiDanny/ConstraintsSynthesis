using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class ConstraintsSatisfaction
    {
        private readonly List<Constraint> _constraints;
        private readonly List<Point> _points;
        private readonly double[][] _constraintsMargins;

        public ConstraintsSatisfaction(List<Constraint> constraints, List<Point> points)
        {
            _constraints = constraints;
            _points = points;

            _constraintsMargins = new double[_constraints.Count][];

            for (var i = 0; i < _constraintsMargins.Length; i++)
            {
                _constraintsMargins[i] = new double[_points.Count];

                for (var j = 0; j < _points.Count; j++)
                {
                    _constraintsMargins[i][j] = _constraints[i].MarginForPoint(_points[j]);
                }
            }
        }

        public IEnumerable<int> GetSatisfyingPointsIndices(Constraint constraint)
        {
            var constraintIndex = _constraints.IndexOf(constraint);

            return GetSatisfyingPointsIndices(constraintIndex);
        }

        public IEnumerable<int> GetUnsatisfyingPointsIndices(Constraint constraint)
        {
            var constraintIndex = _constraints.IndexOf(constraint);

            return GetUnsatisfyingPointsIndices(constraintIndex);
        }

        public IEnumerable<int> GetSatisfyingPointsIndices(int constraintIndex) =>
            GetPointsSatisfyingCondition(constraintIndex, m => m >= 0);

        public IEnumerable<int> GetUnsatisfyingPointsIndices(int constraintIndex) =>
            GetPointsSatisfyingCondition(constraintIndex, m => m < 0);

        private IEnumerable<int> GetPointsSatisfyingCondition(int constraintIndex,
            Predicate<double> condition) =>
                _constraintsMargins[constraintIndex].Select((margin, index) => new {margin, index})
                    .Where(o => condition(o.margin)).Select(o => o.index);
    }
}
