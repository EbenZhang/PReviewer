using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCommentsContainer
    {
        [Test]
        public void TestConvertFromDiffs()
        {
            var file1 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File1"
                })
                {
                    Comments = "Comment1",
                    ReviewStatus = ReviewStatus.Reviewed
                };

            var file2 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File2"
                })
            {
                Comments = "Comment2",
                ReviewStatus = ReviewStatus.HasntBeenReviewed
            };
            var diffs = new List<CommitFileVm>()
            {
                file1,
                file2
            };
            var container = CommentsContainer.From(diffs, "general comments");
            Assert.That(container.GeneralComments, Is.EqualTo("general comments"));
            Assert.That(container.FileComments[0].Comments, Is.EqualTo("Comment1"));
            Assert.That(container.FileComments[0].FileName, Is.EqualTo("File1"));
            Assert.That(container.FileComments[0].ReviewStatus, Is.EqualTo(ReviewStatus.Reviewed));
            
            Assert.That(container.FileComments[1].Comments, Is.EqualTo("Comment2"));
            Assert.That(container.FileComments[1].FileName, Is.EqualTo("File2"));
            Assert.That(container.FileComments[1].ReviewStatus, Is.EqualTo(ReviewStatus.HasntBeenReviewed));
        }

        [Test]
        public void ShouldNotSkipEmptyComments_AsWeNeedToSaveReviewStatus()
        {
            var file1 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File1"
                })
            {
                Comments = "Comment1",
                ReviewStatus = ReviewStatus.HasntBeenReviewed
            };

            var file2 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File2"
                })
            {
                Comments = "",
                ReviewStatus = ReviewStatus.Reviewed
            };

            var file3 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File2"
                })
            {
                Comments = null,
                ReviewStatus = ReviewStatus.ConfirmLater
            };
            var diffs = new List<CommitFileVm>()
            {
                file1,
                file2,
                file3
            };
            var container = CommentsContainer.From(diffs, "general comments");
            Assert.That(container.FileComments[0].Comments, Is.EqualTo("Comment1"));
            Assert.That(container.FileComments[0].FileName, Is.EqualTo("File1"));
            Assert.That(container.FileComments[0].ReviewStatus, Is.EqualTo(ReviewStatus.HasntBeenReviewed));

            Assert.That(container.FileComments[1].Comments, Is.EqualTo(""));
            Assert.That(container.FileComments[1].FileName, Is.EqualTo("File2"));
            Assert.That(container.FileComments[1].ReviewStatus, Is.EqualTo(ReviewStatus.Reviewed));
            
            Assert.That(container.FileComments[2].Comments, Is.EqualTo(null));
            Assert.That(container.FileComments[2].FileName, Is.EqualTo("File2"));
            Assert.That(container.FileComments[2].ReviewStatus, Is.EqualTo(ReviewStatus.ConfirmLater));

            Assert.That(container.FileComments.Count, Is.EqualTo(3));
        }
    }
}
