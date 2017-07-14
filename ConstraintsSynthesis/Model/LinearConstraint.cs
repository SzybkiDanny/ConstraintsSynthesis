using System.Collections.Generic;
using System.Linq;

namespace ConstraintsSynthesis.Model
{
    public class LinearConstraint : Constraint
    {
        public LinearConstraint(double[] coefficients, double absoluteTerm)
        {
            for (var i = 0; i < coefficients.Length; i++)
            {
                Terms[new Term() { [i] = 1.0 }] = coefficients[i];
            }

            AbsoluteTerm = absoluteTerm;
        }

        public LinearConstraint(Dictionary<int, double> coefficients, int dimensions, double absoluteTerm)
        {
            for (var i = 0; i < dimensions; i++)
            {
                Terms[new Term() {[i] = 1.0}] = coefficients.Keys.Contains(i)
                    ? coefficients[i]
                    : 0;
            }

            AbsoluteTerm = absoluteTerm;
        }

        public LinearConstraint Translate(double[] vector)
        {
            var translatedConstraint = Clone() as LinearConstraint;

            for (var i = 0; i < Terms.Count; i++)
            {
                translatedConstraint.AbsoluteTerm += this[i]*vector[i];
            }

            return translatedConstraint;
        }

        public double this[int index]
        {
            get
            {
                var term = new Term() {[index] = 1.0};
                return Terms.ContainsKey(term) ? Terms[term] : 0;
            }
            set { Terms[new Term() { [index] = 1.0 }] = value; }
        }

        public override object Clone()
        {
            return new LinearConstraint(Terms.Values.ToArray(), AbsoluteTerm) {Sign = Sign};
        }
    }
}
