using NUnit.Framework;
using PReviewer.Model;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestDiffToolParamBuilder
    {
        [Test]
        public void TestParamBuilder()
        {
            var builder = new DiffToolParamBuilder();
            var result = builder.Build("Param $Base $Head", "base", "head");
            Assert.That(result, Is.EqualTo("Param \"base\" \"head\""));
        }
    }
}