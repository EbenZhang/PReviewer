using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCommitVm
    {
        [Test]
        public void GivenALongerSHA_ShouldOnlyUseTheFirstFewChars()
        {
            const string sha = "cc7d355564213a3a5sd4f";
            var expectedSha = sha.Substring(0, CommitVm.MaxShaLength);
            var vm = new CommitVm("BASE", "cc7d355564213a3a5sd4f");
            Assert.That(vm.Display, Is.EqualTo(string.Format("#{0} BASE", expectedSha)));
        }

        [Test]
        public void GivenALongerMessage_ShouldOnlyUseTheFirstFewChars()
        {
            var builder = new StringBuilder(CommitVm.MaxMessageLength*2);
            for (var i = 0; i < CommitVm.MaxMessageLength*2; ++i)
            {
                builder.Append("a");
            }
            
            var expectedMessage = builder.ToString().Substring(0, CommitVm.MaxMessageLength) + "...";
            var vm = new CommitVm(builder.ToString(), "cc7d355");
            Assert.That(vm.Display, Is.EqualTo(string.Format("#cc7d355 {0}", expectedMessage)));
        }

        [Test]
        public void GivenAShorterSHA_ShouldProcessCorrectly()
        {
            var vm = new CommitVm("BASE", "cc7");
            Assert.That(vm.Display, Is.EqualTo("#cc7 BASE"));
        }

        [Test]
        public void GivenAShorterMessage_ShouldProcessCorrectly()
        {
            var vm = new CommitVm("BASE", "cc7d355");
            Assert.That(vm.Display, Is.EqualTo("#cc7d355 BASE"));
        }
    }
}
