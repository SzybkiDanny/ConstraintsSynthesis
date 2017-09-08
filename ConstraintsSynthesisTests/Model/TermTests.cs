using System;
using System.Collections.Generic;
using ConstraintsSynthesis.Model;
using NUnit.Framework;

namespace ConstraintsSynthesisTests.Model
{
    [TestFixture]
    public class TermTests
    {
        private static readonly object[] ValueInValidPointTestCases =
        {
            new object[] {new Dictionary<int, double>(), new Point(new[] {1.0, 2.0}), 0.0},
            new object[] {new Dictionary<int, double> {{0, 3.0}}, new Point(new[] {2.0, 2.0}), 8.0},
            new object[]
                {new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}, new Point(new[] {2.0, 2.0, 3.0}), 96.0},
            new object[]
                {new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}, new Point(new[] {0.0, 0.0, 0.0}), 0.0}
        };

        private static readonly object[] ValueInInvalidPointTestCases =
        {
            new object[] {new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}, new Point(new[] {2.0, 2.0})},
            new object[] {new Dictionary<int, double> {{0, 1.0}, {1, 1.0}, {3, 1.0}}, new Point(new[] {2.0, 2.0, 2.0})}
        };

        private static readonly object[] ToStringTestCases =
        {
            new object[] {new Dictionary<int, double> {{0, 3.0}, {1, 2.0}, {2, 1.0}}, "x0^3 x1^2 x2^1"},
            new object[] {new Dictionary<int, double> {{0, 1.0}, {1, 1.0}, {3, 1.0}}, "x0^1 x1^1 x3^1"},
            new object[] {new Dictionary<int, double> {{0, 1.5}, {1, 1.2}, {3, -10.2}}, "x0^1.5 x1^1.2 x3^-10.2"},
            new object[] {new Dictionary<int, double> {{0, 1.5}, {1, 0}, {3, -10.2}}, "x0^1.5 x3^-10.2"},
            new object[] {new Dictionary<int, double> {{0, 1.5}, {3, -10.2}, {1, 2}}, "x0^1.5 x1^2 x3^-10.2"}
        };

        private Term _term;

        [Test]
        public void GetVariableThatIsNotPartOfTheTermTest()
        {
            var exponent = _term[0];

            Assert.AreEqual(0, exponent, double.Epsilon);
        }

        [Test]
        [TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTest(Dictionary<int, double> variables, string expected)
        {
            _term = new Term(variables);

            Assert.AreEqual(expected, _term.ToString());
        }

        [Test]
        [TestCaseSource(nameof(ValueInInvalidPointTestCases))]
        public void ValueInInvalidPointTest(Dictionary<int, double> variables, Point point)
        {
            _term = new Term(variables);

            Assert.Throws(typeof(IndexOutOfRangeException), () => _term.Value(point));
        }

        [Test]
        [TestCaseSource(nameof(ValueInValidPointTestCases))]
        public void ValueInValidPointTest(Dictionary<int, double> variables, Point point, double expectedValue)
        {
            _term = new Term(variables);

            var actualValue = _term.Value(point);

            Assert.AreEqual(expectedValue, actualValue, double.Epsilon);
        }
    }
}