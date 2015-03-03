using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using NUnit.Framework;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestPullRequestUrlConverter
    {
        [Test]
        public void GivenAValidUrl_ShouldParseCorrectly()
        {
            var locator = PullRequestLocator.FromUrl(@"https://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));

            locator = PullRequestLocator.FromUrl(@"https://github.com/EbenZhang1/EZPlayer1/pull/184");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang1"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer1"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(184));
        }

        [Test]
        public void CanProcessProtocolInACaseInsensitiveManner()
        {
            var locator = PullRequestLocator.FromUrl(@"Https://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));

            locator = PullRequestLocator.FromUrl(@"HtTps://github.com/EbenZhang1/EZPlayer1/pull/184");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang1"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer1"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(184));
        }

        [Test]
        public void GivenAUrlWithNoProtocol_ShouldParseCorretly()
        {
            var locator = PullRequestLocator.FromUrl(@"github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));
        }

        [Test]
        public void CanProcessHttpAsWellAsHttps()
        {
            var locator = PullRequestLocator.FromUrl(@"Http://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));
        }

        [Test]
        public void CanConvertToUrl()
        {
            var locator = new PullRequestLocator()
            {
                Owner = "EbenZhang",
                PullRequestNumber = 100,
                Repository = "PReviewer",
            };
            var url = locator.ToUrl();
            Assert.That(url, Is.EqualTo("https://github.com/EbenZhang/PReviewer/pull/100"));
        }
    }
}
