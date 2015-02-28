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
            var locator = PullRequestUrlConverter.PullRequestLocator(@"https://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));

            locator = PullRequestUrlConverter.PullRequestLocator(@"https://github.com/EbenZhang1/EZPlayer1/pull/184");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang1"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer1"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(184));
        }

        [Test]
        public void CanProcessProtocolInACaseInsensitiveManner()
        {
            var locator = PullRequestUrlConverter.PullRequestLocator(@"Https://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));

            locator = PullRequestUrlConverter.PullRequestLocator(@"HtTps://github.com/EbenZhang1/EZPlayer1/pull/184");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang1"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer1"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(184));
        }

        [Test]
        public void GivenAUrlWithNoProtocol_ShouldParseCorretly()
        {
            var locator = PullRequestUrlConverter.PullRequestLocator(@"github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));
        }

        [Test]
        public void CanProcessHttpAsWellAsHttps()
        {
            var locator = PullRequestUrlConverter.PullRequestLocator(@"Http://github.com/EbenZhang/EZPlayer/pull/183");
            Assert.That(locator.Owner, Is.EqualTo("EbenZhang"));
            Assert.That(locator.Repository, Is.EqualTo("EZPlayer"));
            Assert.That(locator.PullRequestNumber, Is.EqualTo(183));
        }
    }

    public static class PullRequestUrlConverter
    {
        public static PullRequestLocator PullRequestLocator(string url)
        {
            url = Regex.Replace(url, @"HTTPS://|http://", "", RegexOptions.IgnoreCase);
            var splited = url.Split(new char[]{'/'});
            return new PullRequestLocator()
            {
                Owner = splited[1],
                Repository = splited[2],
                PullRequestNumber = int.Parse(splited[4])
            };
        }
    }
}
