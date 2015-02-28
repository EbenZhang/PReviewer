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

        private PullRequestLocator _pullRequestLocator = new PReviewer.Domain.PullRequestLocator()
        {
            Repository = "repo",
            Owner = "owner",
            PullRequestNumber = new Random().Next()
        };

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
                PullRequestLocator = _pullRequestLocator,
                IsUrlMode = false
            };

            _pullRequest = new MockPullRequest();
            _prClient.Get(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository, _mainWindowVm.PullRequestLocator.PullRequestNumber).Returns(Task.FromResult((PullRequest)_pullRequest));
        }

        [Test]
        public async void ShouldBeAbleToGetDiffsForPullRequest()
        {
            await _mainWindowVm.RetrieveDiffs();

#pragma warning disable 4014
            _prClient.Received(1).Get(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository, _mainWindowVm.PullRequestLocator.PullRequestNumber);
            _commitsClient.Received(1).Compare(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository, _pullRequest.Base.Sha, _pullRequest.Head.Sha);
#pragma warning restore 4014
            Assert.That(_mainWindowVm.Diffs, Contains.Item(_compareResults.File1));
            Assert.That(_mainWindowVm.Diffs, Contains.Item(_compareResults.File2));
        }

        [Test]
        public async void ShouldUpdateBusyStatusProperly()
        {
            var updateCount = 0;
            _mainWindowVm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((MainWindowVm x) => x.IsProcessing))
                {
                    updateCount++;
                }
            };
            await _mainWindowVm.RetrieveDiffs();

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
            
            Assert.Throws<Exception>(async () => await _mainWindowVm.RetrieveDiffs());

            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public void ShouldBeInUrlModeByDefault()
        {
            Assert.True(new MainWindowVm(_gitHubClient).IsUrlMode);
        }

        [Test]
        public async void ShouldUpdatePRLocator_WhenRetrieveChangesInUrlMode()
        {
            _mainWindowVm.IsUrlMode = true;
            _mainWindowVm.PullRequestUrl = string.Format(@"https://github.com/{0}/{1}/pull/{2}",
                _pullRequestLocator.Owner,
                _pullRequestLocator.Repository,
                _pullRequestLocator.PullRequestNumber);
            _mainWindowVm.PullRequestLocator.Owner = "AnotherOwner";
            _mainWindowVm.PullRequestLocator.Repository = "AnotherRepo";
            _mainWindowVm.PullRequestLocator.PullRequestNumber = _pullRequestLocator.PullRequestNumber + 11;

            await _mainWindowVm.RetrieveDiffs();
            Assert.That(_mainWindowVm.PullRequestLocator.Owner, Is.EqualTo(_pullRequestLocator.Owner));
            Assert.That(_mainWindowVm.PullRequestLocator.Repository, Is.EqualTo(_pullRequestLocator.Repository));
            Assert.That(_mainWindowVm.PullRequestLocator.PullRequestNumber, Is.EqualTo(_pullRequestLocator.PullRequestNumber));
        }

        [Test]
#pragma warning disable 1998
        public async void GivenAnInvalidUrl_ShouldThrowAnException()
#pragma warning restore 1998
        {
            _mainWindowVm.IsUrlMode = true;
            _mainWindowVm.PullRequestUrl = "";
            Assert.Throws<UriFormatException>(async () => await _mainWindowVm.RetrieveDiffs());

            _mainWindowVm.PullRequestUrl = "asl;dfkjasldf";
            Assert.Throws<UriFormatException>(async () => await _mainWindowVm.RetrieveDiffs());
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
