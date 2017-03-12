using System.Collections.Generic;
using System.Linq;
using ConstraintsSynthesis.Model;
using NUnit.Framework;

namespace ConstraintsSynthesisTests.Model
{
    [TestFixture]
    public class ConstraintTests
    {
        [SetUp]
        public void Init()
        {
            _constraint = new Constraint();
        }

        private static readonly object[] ChangeInequalityDirectionTestCases =
        {
            new object[] {new List<double> {1.0, -2.0, 1.543, -0.23423}, 6.43},
            new object[] {new List<double> {1.0, -2.0, 1.543, 0.0}, 0.0}
        };

        private static readonly object[] IsSatisfyingTestCases =
        {
            new object[]
            {
                new Dictionary<Term, double> {{new Term(new Dictionary<int, double> {{0, 3.0}}), 2.0}}, 2.0,
                new Point(new[] {2.0, 3.0, 3.0}), false
            },
            new object[]
            {
                new Dictionary<Term, double>
                {
                    {new Term(new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}), -4.5},
                    {new Term(new Dictionary<int, double> {{0, 2.0}, {1, 1.0}}), 1.5}
                },
                -3.2,
                new Point(new[] {2.0, 3.0, 3.0}),
                true
            }
        };

        private static readonly object[] ToStringTestCases =
        {
            new object[]
            {
                new Dictionary<Term, double> {{new Term(new Dictionary<int, double> {{0, 3.0}}), 2.0}}, 2.0, "2 * x0^3",
            },
            new object[]
            {
                new Dictionary<Term, double>
                {
                    {new Term(new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}), -4.5},
                    {new Term(new Dictionary<int, double> {{0, 2.0}, {1, 1.0}}), 1.5}
                },
                -3.2,
                "-4.5 * x0^3 x1^2 x2^1 + 1.5 * x0^2 x1^1"
            },
            new object[]
            {
                new Dictionary<Term, double>
                {
                    {new Term(new Dictionary<int, double> {{0, 3.0}, {1, 0.0}, {2, 1.0}}), -4.5},
                    {new Term(new Dictionary<int, double> {{0, 2.0}, {1, 1.0}}), 0.0}
                },
                -3.2,
                "-4.5 * x0^3 x2^1"
            }
        };

        private Constraint _constraint;

        [Test]
        [TestCaseSource(nameof(ChangeInequalityDirectionTestCases))]
        public void ChangeInequalityDirectionTest(List<double> coefficients, double absoluteTerm)
        {
            _constraint.AbsoluteTerm = absoluteTerm;

            for (var i = 0; i < coefficients.Count; i++)
                _constraint.Terms[new Term(new Dictionary<int, double> {{i, coefficients[i]}})] = coefficients[i];

            _constraint.ChangeInequalityDirection();

            var newCoefficients = _constraint.Terms.Values.ToArray();

            for (var i = 0; i < coefficients.Count; i++)
                Assert.AreEqual(0, coefficients[i] + newCoefficients[i], double.Epsilon);

            Assert.AreEqual(0, absoluteTerm + _constraint.AbsoluteTerm, double.Epsilon);
        }

        [Test]
        [TestCaseSource(nameof(IsSatisfyingTestCases))]
        public void IsSatisfyingTest(Dictionary<Term, double> terms, double absoluteTerm, Point point, bool expected)
        {
            _constraint.AbsoluteTerm = absoluteTerm;

            foreach (var term in terms)
                _constraint.Terms[term.Key] = term.Value;

            var actual = _constraint.IsSatisfying(point);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTest(Dictionary<Term, double> terms, double absoluteTerm, string expected)
        {
            _constraint.AbsoluteTerm = absoluteTerm;

            foreach (var term in terms)
                _constraint.Terms[term.Key] = term.Value;

            var actual = _constraint.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}