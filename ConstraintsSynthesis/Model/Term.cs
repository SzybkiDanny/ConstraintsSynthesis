using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class Term
    {
        private readonly SortedDictionary<int, double> _variables;

        public Term() : this(null)
        {
        }

        public Term(Dictionary<int, double> variables)
        {
            _variables = new SortedDictionary<int, double>(variables ?? new Dictionary<int, double>());
        }

        public double this[int index]
        {
            get { return _variables.ContainsKey(index) ? _variables[index] : 0; }
            set { _variables[index] = value; }
        }

        public double Value(Point point) =>
            _variables.Aggregate(_variables.Count >0 ? 1.0 : 0.0,
                (current, entry) => current*Math.Pow(point[entry.Key], entry.Value));

        public override string ToString() =>
            string.Join(" ", _variables.Where(entry => Math.Abs(entry.Value) > double.Epsilon).Select(entry => $"x{entry.Key}^{entry.Value}"));

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