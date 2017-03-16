using System;
using ConstraintsSynthesis.Model;
using NUnit.Framework;

namespace ConstraintsSynthesisTests.Model
{
    [TestFixture]
    public class PointTests
    {
        private static readonly object[] ComparisonOperatorsTestCases =
        {
            new object[] {new [] {1.0, 2.0, 3.0}, new[] { 1.0, 2.0, 3.0 }, false, false},
            new object[] {new [] {0.4, 1.4, 2.9}, new[] { 1.0, 2.0, 3.0 }, true, false},
            new object[] {new [] {0.4, 1.4, 3.0}, new[] { 1.0, 2.0, 3.0 }, false, false},
            new object[] {new [] {-0.4, -1.4, -2.9}, new[] { -1.0, -2.0, -3.0 }, false, true},
        };

        private static readonly object[] ComparisonOperatorsExceptionTestCases =
        {
            new object[] {new [] {1.0, 2.0}, new[] { 1.0, 2.0, 3.0 }},
            new object[] {new [] {0.4, 1.4, 2.9}, new[] { 1.0, 2.0}},
        };

        private static readonly object[] ToStringTestCases =
        {
            new object[] {new [] {1.0, 2.0, 3.0}, true, "1, 2, 3 True"},
            new object[] {new [] {1.3, 2.234234, -43.0}, false, "1.3, 2.234234, -43 False"},
            new object[] {new [] {0.0001, 0, 99999999999}, false, "0.0001, 0, 99999999999 False" }
        };

        [Test]
        [TestCaseSource(nameof(ComparisonOperatorsTestCases))]
        public void ComparisonOperatorsTest(double[] coordinates1, double[] coordinates2, bool isLessThan, bool isGreaterThan)
        {
            var point1 = new Point(coordinates1);
            var point2 = new Point(coordinates2);

            Assert.AreEqual(isLessThan, point1 < point2);
            Assert.AreEqual(isGreaterThan, point1 > point2);
        }

        [Test]
        [TestCaseSource(nameof(ComparisonOperatorsExceptionTestCases))]
        public void ComparisonOperatorsExpceptionTest(double[] coordinates1, double[] coordinates2)
        {
            var point1 = new Point(coordinates1);
            var point2 = new Point(coordinates2);

            Assert.Throws(typeof(ArgumentException), () => { var result = point1 < point2; });
            Assert.Throws(typeof(ArgumentException), () => {var result = point1 > point2;});
        }

        [Test]
        [TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTest(double[] coordinates, bool label, string expected)
        {
            var point = new Point(coordinates) {Label = label};

            Assert.AreEqual(expected, point.ToString());
        }
    }
}