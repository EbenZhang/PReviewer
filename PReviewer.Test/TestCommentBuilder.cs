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
    public class TestCommentBuilder
    {
        private const string MyGeneralComments = "My General Comments";
        private const string ExpectedComment1 = @"###### *test.xaml*

comment1";
        private const string ExpectedComment2 = @"###### *app.xaml.cs*

comment2";
        private const string ExpectedGeneralComments = @"###### *General Comments*

My General Comments";

        private readonly MockGitHubCommitFile _gitHubCommitFile1 = new MockGitHubCommitFile
        {
            Sha = "e74fe8d371a5e33c4877f662e6f8ed7c0949a8b0",
            Filename = "test.xaml",
            Patch = "Patch",
        };

        private readonly MockGitHubCommitFile _gitHubCommitFile2 = new MockGitHubCommitFile
        {
            Sha = "9dc7f01526e368a64c49714c51f1d851885793ba",
            Filename = "app.xaml.cs"
        };

        private CommitFileVm _commitFileVm1;
        private CommitFileVm _commitFileVm2;
        private CommentsBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _commitFileVm1 = new CommitFileVm(_gitHubCommitFile1)
            {
                Comments = "comment1"
            };

            _commitFileVm2 = new CommitFileVm(_gitHubCommitFile2)
            {
                Comments = "comment2"
            };

            _builder = new CommentsBuilder();
        }

        [Test]
        public void HappyPath()
        {   
            var files = new List<CommitFileVm>
            {
                _commitFileVm1,
                _commitFileVm2
            };

            var expectedComments = string.Join("\r\n", ExpectedComment1, ExpectedComment2, ExpectedGeneralComments);

            var comments = _builder.Build(files, MyGeneralComments).ToArray();

            Assert.That(comments, Is.EqualTo(expectedComments));
        }

        [Test]
        public void ShouldSkipEmptyComments()
        {
            _commitFileVm2.Comments = "";
            var files = new List<CommitFileVm>
            {
                _commitFileVm1,
                _commitFileVm2
            };

            var expectedComments = string.Join("\r\n", ExpectedComment1, ExpectedGeneralComments);

            var comments = _builder.Build(files, MyGeneralComments).ToArray();
            Assert.That(comments, Is.EqualTo(expectedComments));

            comments = _builder.Build(files, "").ToArray();
            Assert.That(comments, Is.EqualTo(ExpectedComment1));
        }
    }
}
