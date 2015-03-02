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
    public class TestCommitFileVm
    {
        [Test]
        public void TestNextStatus()
        {
            var vm = new CommitFileVm(new MockGitHubCommitFile()) {ReviewStatus = ReviewStatus.Reviewed};
            vm.ChangeReviewStatusCmd.Execute(null);
            Assert.That(vm.ReviewStatus, Is.EqualTo(ReviewStatus.ConfirmLater));
            vm.ChangeReviewStatusCmd.Execute(null);
            Assert.That(vm.ReviewStatus, Is.EqualTo(ReviewStatus.HasntBeenReviewed));
            vm.ChangeReviewStatusCmd.Execute(null);
            Assert.That(vm.ReviewStatus, Is.EqualTo(ReviewStatus.Reviewed));
        }
    }
}
