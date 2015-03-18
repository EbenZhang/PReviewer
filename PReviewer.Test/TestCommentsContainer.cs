using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;
using Shouldly;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCommentsContainer
    {
        private CommitFileVm _file1;
        private CommitFileVm _file2;
        private CommitFileVm _file3;

        [SetUp]
        public void SetUp()
        {
            _file1 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File1"
                })
            {
                Comments = "Comment1",
                ReviewStatus = ReviewStatus.Reviewed
            };

            _file2 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File2"
                })
            {
                Comments = "Comment2",
                ReviewStatus = ReviewStatus.HasntBeenReviewed
            };

            _file3 = new CommitFileVm(
                new MockGitHubCommitFile
                {
                    Filename = "File3"
                })
            {
                Comments = null,
                ReviewStatus = ReviewStatus.ConfirmLater
            };
        }
        [Test]
        public void HappyPath()
        {
            var diffs = new List<CommitFileVm>()
            {
                _file1,
                _file2
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
            var diffs = new List<CommitFileVm>
            {
                _file1,
                _file2,
                _file3
            };
            _file2.Comments = "";
            var container = CommentsContainer.From(diffs, "general comments");

            container.FileComments[1].Comments.ShouldBeNullOrEmpty();
            container.FileComments[1].FileName.ShouldBe("File2");

            container.FileComments.Count.ShouldBe(3);

            container.FileComments[2].Comments.ShouldBeNullOrEmpty();
            container.FileComments[2].FileName.ShouldBe("File3");
        }

        [Test]
        public void AbleToAddComments_SoCommentsWillBeKept_WhenUserChangeCommitRange()
        {
            var diffs = new List<CommitFileVm>()
            {
                _file1,
                _file2,
            };

            var container = CommentsContainer.From(diffs, "general comments");

            var commentsInCommitRange = new List<CommitFileVm>
            {// _file2 is not in commit range, _file3 is new.
                _file1,
                _file3
            };
            container.AddComments(commentsInCommitRange, "new general comments");
            container.GeneralComments.ShouldBe("new general comments");
            container.FileComments.ShouldContain(x => x.FileName == _file1.GitHubCommitFile.Filename);
            container.FileComments.ShouldContain(x => x.FileName == _file2.GitHubCommitFile.Filename);
            container.FileComments.ShouldContain(x => x.FileName == _file3.GitHubCommitFile.Filename);
            container.FileComments.ShouldContain(x => x.Comments == _file3.Comments);
        }
    }
}
