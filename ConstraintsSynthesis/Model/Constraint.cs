using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    class Constraint
    {
        public Dictionary<Term, double> Terms { get; } = new Dictionary<Term, double>();

        public double AbsoluteTerm { get; set; } = 1;

        public void ChangeInequalityDirection()
        {
            AbsoluteTerm *= -1;

            foreach (var term in Terms.ToList())
            {
                Terms[term.Key] *= -1;
            }
        }

        public bool IsSatisfying(Point point) =>
            Terms.Sum(entry => entry.Key.Value(point)*entry.Value) <= AbsoluteTerm;

        public override bool Equals(object obj)
        {
            return Equals(obj as Constraint);
        }

        public bool Equals(Constraint constraint)
        {
            if (constraint == null)
                return false;

            if (ReferenceEquals(this, constraint))
                return true;

            if (GetHashCode() != constraint.GetHashCode())
                return false;

            return Terms.Equals(constraint.Terms) && AbsoluteTerm == constraint.AbsoluteTerm;
        }

        public override int GetHashCode()
        {
            return (int)Terms.Select(entry => entry.Key.GetHashCode() * entry.Value).Sum();
        }

        public override string ToString()
        {
            return string.Join(" + ", Terms.Where(t => Math.Abs(t.Value) > double.Epsilon).Select(t => $"{t.Value} * {t.Key}"));
        }
    }
}
