using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtendedCL;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestMainWindowVm
    {
        private IGitHubClient _gitHubClient;
        private IRepositoriesClient _repoClient;
        private IRepositoryCommitsClient _commitsClient;
        private IPullRequestsClient _prClient;
        private MockCompareResult _compareResults;
        private MainWindowVm _mainWindowVm;
        private MockPullRequest _pullRequest;

        [SetUp]
        public void SetUp()
        {
            _compareResults = new MockCompareResult();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _repoClient = Substitute.For<IRepositoriesClient>();
            _commitsClient = Substitute.For<IRepositoryCommitsClient>();
            _prClient = Substitute.For<IPullRequestsClient>();
            _gitHubClient.Repository.Returns(_repoClient);
            _repoClient.Commits.Returns(_commitsClient);
            _repoClient.PullRequest.Returns(_prClient);

            _commitsClient.Compare(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>()
                ).Returns(Task.FromResult((CompareResult)_compareResults));

            _mainWindowVm = new MainWindowVm(_gitHubClient)
            {
                Repository = "repo",
                Owner = "owner",
                PullRequestNumber = 11,
            };

            _pullRequest = new MockPullRequest();
            _prClient.Get(_mainWindowVm.Owner, _mainWindowVm.Repository, _mainWindowVm.PullRequestNumber).Returns(Task.FromResult((PullRequest)_pullRequest));
        }

        [Test]
        public void ShouldBeAbleToGetDiffsForPullRequest()
        {
            _mainWindowVm.RetrieveDiffs();

            _prClient.Received(1).Get(_mainWindowVm.Owner, _mainWindowVm.Repository, _mainWindowVm.PullRequestNumber);
            _commitsClient.Received(1).Compare(_mainWindowVm.Owner, _mainWindowVm.Repository, _pullRequest.Base.Sha, _pullRequest.Head.Sha);
            Assert.That(_mainWindowVm.Diffs, Contains.Item(_compareResults.File1));
            Assert.That(_mainWindowVm.Diffs, Contains.Item(_compareResults.File2));
        }

        [Test]
        public void ShouldUpdateBusyStatusProperly()
        {
            var updateCount = 0;
            _mainWindowVm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((MainWindowVm x) => x.IsProcessing))
                {
                    updateCount++;
                }
            };
            _mainWindowVm.RetrieveDiffs();

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public void GivenAnException_BusyStatusShouldBeReset()
        {
            _prClient.When(x => x.Get(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int>()))
                .Do(x =>
                {
                    throw new Exception();
                });
            
#pragma warning disable 1998
            Assert.Throws<Exception>(async () => _mainWindowVm.RetrieveDiffs());
#pragma warning restore 1998

            Assert.False(_mainWindowVm.IsProcessing);
        }
    }

    class MockPullRequest : PullRequest
    {
        public MockPullRequest()
        {
            Base = new GitReference("", "", "", "1212", null, null);
            Head = new GitReference("", "", "", "asdfasdf", null, null);
        }
    }

    class MockCompareResult : CompareResult
    {
        public GitHubCommitFile File1 = new GitHubCommitFile();
        public GitHubCommitFile File2 = new GitHubCommitFile();
        public MockCompareResult()
        {
            Files = new List<GitHubCommitFile>()
            {
                File1,
                File2
            };
        }
    }
}
