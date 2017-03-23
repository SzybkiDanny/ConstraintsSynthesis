namespace ConstraintsSynthesis.Model
{
    public class LinearConstraint : Constraint
    {
        public LinearConstraint(double[] coefficients, double absoluteTerm)
        {
            for (int i = 0; i < coefficients.Length; i++)
            {
                Terms[new Term() { [i] = 1.0 }] = coefficients[i];
            }

            AbsoluteTerm = absoluteTerm;
        }
    }
}
