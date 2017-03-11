using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    internal abstract class Term
    {
        private readonly SortedDictionary<int, double> _variables =
            new SortedDictionary<int, double>();

        public double this[int index]
        {
            get { return _variables[index]; }
            set { _variables[index] = value; }
        }

        public double Value(Point point) =>
            _variables.Aggregate(1.0, (current, entry) => current*Math.Pow(point[entry.Key], entry.Value));

        public override string ToString() =>
            string.Join(" ", _variables.Select(entry => $"x{entry.Key}^{entry.Value}"));

        public override bool Equals(object obj) =>
            Equals(obj as Term);

        public bool Equals(Term term)
        {
            if (term == null)
                return false;

            if (ReferenceEquals(this, term))
                return true;

            if (GetHashCode() != term.GetHashCode())
                return false;

            return string.Equals(ToString(), term.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}