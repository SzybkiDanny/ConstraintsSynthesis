using System;
using System.Linq;
using NUnit.Framework;
using TestDataGenerator.Generators;

namespace TestDataGeneratorTests.Generators
{
    [TestFixture]
    public class SphereGeneratorTests
    {
        [SetUp]
        public void Init()
        {
            _sphereGenerator = new SphereGenerator();
        }

        private static readonly object[] PositiveAndNegativeLabelsCountCases =
        {
            new object[] {3127, 518},
            new object[] {1085, 2504}
        };

        private static readonly object[] CorrectLabelsAssignedTestCases =
        {
            new object[] {7, new double[] {10, 12, 11, 15}, 3127, 518},
            new object[] {10, new double[] {10, 12, 11, 15, 19, 20, 10}, 31270, 5180},
            new object[] {10, new double[] {10, 12, 11, 15, 19, 20, 10}, 3127, 51800},
        };

        private SphereGenerator _sphereGenerator;

        [Test, TestCaseSource(nameof(CorrectLabelsAssignedTestCases))]
        public void CorrectLabelsAssigned(double radius, double[] center, int positives,
            int negatives)
        {
            var data = _sphereGenerator.Generate(center.Length, radius, center, positives, negatives);

            foreach (var point in data)
            {
                var correctLabel =
                    Math.Sqrt(point.Coordinates.Zip(center, (d1, d2) => Math.Pow(d1 - d2, 2)).Sum()) <=
                    radius;
                Assert.AreEqual(point.Label, correctLabel);
            }
        }

        [Test, TestCaseSource(nameof(PositiveAndNegativeLabelsCountCases))]
        public void PositiveAndNegativeLabelsCount(int positivesExpected, int negativesExpected)
        {
            var data = _sphereGenerator.Generate(3, 5, new double[] {0, 0, 0}, positivesExpected,
                negativesExpected);

            var positivesActual = data.Count(p => p.Label);
            var negativesActual = data.Count(p => !p.Label);

            Assert.AreEqual(positivesExpected, positivesActual);
            Assert.AreEqual(negativesExpected, negativesActual);
        }
    }
}