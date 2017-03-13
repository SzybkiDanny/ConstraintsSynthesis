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
                new Dictionary<Term, double> {{new Term(new Dictionary<int, double> {{0, 3.0}}), 2.0}}, 2.0, Inequality.GreaterThanOrEqual, "2 * x0^3 >= 2",
            },
            new object[]
            {
                new Dictionary<Term, double>
                {
                    {new Term(new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}), -4.5},
                    {new Term(new Dictionary<int, double> {{0, 2.0}, {1, 1.0}}), 1.5}
                },
                -3.2,
                Inequality.LessThanOrEqual,
                "-4.5 * x0^3 x1^2 x2^1 + 1.5 * x0^2 x1^1 <= -3.2"
            },
            new object[]
            {
                new Dictionary<Term, double>
                {
                    {new Term(new Dictionary<int, double> {{0, 3.0}, {1, 0.0}, {2, 1.0}}), -4.5},
                    {new Term(new Dictionary<int, double> {{0, 2.0}, {1, 1.0}}), 0.0}
                },
                4.2,
                Inequality.GreaterThanOrEqual,
                "-4.5 * x0^3 x2^1 >= 4.2"
            }
        };

        private Constraint _constraint;

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
        public void ToStringTest(Dictionary<Term, double> terms, double absoluteTerm, Inequality sign, string expected)
        {
            _constraint.AbsoluteTerm = absoluteTerm;
            _constraint.Sign = sign;

            foreach (var term in terms)
                _constraint.Terms[term.Key] = term.Value;

            var actual = _constraint.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}