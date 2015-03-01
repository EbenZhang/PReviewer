using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PReviewer.Domain;
using PReviewer.Model;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCompareToolSettings
    {
        [Test]
        public void TestConvertToSerializable()
        {
            var compareTool = new CompareToolSettings();
            var serializable = compareTool.ToSerializable();
            Assert.That(serializable.Name, Is.EqualTo(compareTool.Name));
            Assert.That(serializable.Parameters, Is.EqualTo(compareTool.Parameters));
            Assert.That(serializable.ExePath, Is.EqualTo(compareTool.ExePath));
        }

        [Test]
        public void TestConvertFromSerializable()
        {
            var serializable = new SerializableCompareToolSettings()
            {
                Name = "111",
                Parameters = "222",
                ExePath = "333"
            };

            var compareTool = new CompareToolSettings();
            compareTool.UpdateFrom(serializable);

            Assert.That(serializable.Name, Is.EqualTo(compareTool.Name));
            Assert.That(serializable.Parameters, Is.EqualTo(compareTool.Parameters));
            Assert.That(serializable.ExePath, Is.EqualTo(compareTool.ExePath));
        }

        [Test]
        public void TestCompareFunction()
        {
            var tool1 = new CompareToolSettings()
            {
                Name = "1",
                Parameters = "2",
                ExePath = "3"
            };

            var tool2 = new CompareToolSettings()
            {
                Name = "1",
                Parameters = "2",
                ExePath = "3"
            };

            Assert.That(tool1, Is.EqualTo(tool2));

            var tool3 = new CompareToolSettings()
            {
                Name = "0",
                Parameters = "2",
                ExePath = "3"
            };
            Assert.Greater(tool1.CompareTo(tool3), 0);

            var tool4 = new CompareToolSettings()
            {
                Name = "1",
                Parameters = "0",
                ExePath = "3"
            };

            Assert.Greater(tool1.CompareTo(tool4), 0);

            var tool5 = new CompareToolSettings()
            {
                Name = "1",
                Parameters = "1",
                ExePath = "2"
            };

            Assert.Greater(tool1.CompareTo(tool5), 0);
        }

        [Test]
        public void ParametersHaveDefaultValue()
        {
            var settings = new CompareToolSettings();
            Assert.That(settings.Parameters, Is.EqualTo(CompareToolSettings.DefaultParameters));
        }
    }
}
