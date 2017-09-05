using System;
using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class Term
    {
        private readonly string _termString;
        private readonly SortedDictionary<int, double> _variables;

        public Term() : this(null)
        {
        }

        public Term(int index, double value = 1.0) : this(null)
        {
            _variables[index] = value;
        }

        public Term(Dictionary<int, double> variables)
        {
            _variables = new SortedDictionary<int, double>(variables ?? new Dictionary<int, double>());
            _termString = string.Join(" ",
                _variables.Where(entry => Math.Abs(entry.Value) > double.Epsilon)
                    .Select(
                        entry =>
                            entry.Key + (Math.Abs(entry.Value - 1.0) > double.Epsilon
                                ? " ^ " + entry.Value
                                : string.Empty)));
        }

        public double this[int index] => _variables.ContainsKey(index) ? _variables[index] : 0;

        public double Value(Point point) =>
            _variables.Aggregate(_variables.Count > 0 ? 1.0 : 0.0,
                (current, entry) => current * Math.Pow(point[entry.Key], entry.Value));

        public override string ToString() => _termString;

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