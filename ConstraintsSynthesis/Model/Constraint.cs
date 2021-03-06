using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace ConstraintsSynthesis.Model
{
    public class Constraint : ICloneable
    {
        public Dictionary<Term, double> Terms { get; } = new Dictionary<Term, double>();
        public double AbsoluteTerm { get; set; } = 1;
        public Inequality Sign { get; set; } = Inequality.LessThanOrEqual;
        public bool IsMarkedRedundant { get; set; }

        public double this[Term index]
        {
            get { return Terms.ContainsKey(index) ? Terms[index] : 0; }
            set { Terms[index] = value; }
        }

        public bool IsSatisfying(Point point) =>
            MarginForPoint(point) >= 0;

        public double ValueForPoint(Point point) =>
            Terms.Sum(entry => entry.Key.Value(point)*entry.Value);

        public double MarginForPoint(Point point) =>
            Sign == Inequality.LessThanOrEqual
                ? AbsoluteTerm - ValueForPoint(point)
                : ValueForPoint(point) - AbsoluteTerm;

        public double DistanceFromPoint(Point point) =>
            Math.Abs(ValueForPoint(point) + AbsoluteTerm) * Terms.Values.ToArray().Dot(Terms.Values.ToArray());

        public void InvertInequalitySign() =>
            Sign = (Inequality) ((int)(Sign+1) % 2);

        public void ConvertToLessThanOrEqual()
        {
            if (Sign == Inequality.LessThanOrEqual)
                return;

            foreach (var term in Terms.Keys.ToList())
            {
                Terms[term] *= -1;
            }

            Sign = Inequality.LessThanOrEqual;
            AbsoluteTerm *= -1;
        }

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
            return (int) Terms.Select(entry => entry.Key.GetHashCode() * entry.Value).Sum();
        }

        public virtual object Clone()
        {
            var clonedConstrained = new Constraint() {
                AbsoluteTerm = AbsoluteTerm,
                Sign = Sign,
                IsMarkedRedundant = IsMarkedRedundant
            };

            foreach (var term in Terms)
            {
                clonedConstrained.Terms.Add(term.Key, term.Value);
            }

            return clonedConstrained;
        }

        public override string ToString()
        {
            return
                $"{string.Join(" + ", Terms.Where(t => Math.Abs(t.Value) > double.Epsilon).Select(t => $"{t.Value} * {t.Key}"))} " +
                $"{(Sign == Inequality.LessThanOrEqual ? "<=" : ">=")} {AbsoluteTerm}";
        }
    }
}