using ConstraintsSynthesis.Model;
using NUnit.Framework;

namespace ConstraintsSynthesisTests.Model
{
    [TestFixture]
    public class TermTests
    {
        [SetUp]
        public void Init()
        {
            _term = new Term();
        }

        private static readonly object[] PositiveAndNegativeLabelsCountCases =
        {
            new object[] {3127, 518},
            new object[] {1085, 2504}
        };

        private Term _term;

        [Test]
        public void GetVariableThatIsNotPartOfTheTermTests()
        {
            var exponent = _term[0];

            Assert.AreEqual(0, exponent, double.Epsilon);
        }
    }
}
