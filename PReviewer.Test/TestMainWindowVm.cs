using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ExtendedCL;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestMainWindowVm
    {
        private const string Comment1 = "Comment1";
        private const string Comment2 = "Comment2";
        private const string GeneralComments = "general comment";
        private const ReviewStatus ReviewStatus1 = ReviewStatus.Reviewed;
        private const ReviewStatus ReviewStatus2 = ReviewStatus.HasntBeenReviewed;

        private readonly PullRequestLocator _pullRequestLocator = new PullRequestLocator
        {
            Repository = "repo",
            Owner = "owner",
            PullRequestNumber = new Random().Next()
        };

        private string _baseFileName;
        private ICommentsBuilder _commentsBuilder;
        private CommentsContainer _commentsContainer;
        private ICommentsPersist _commentsPersist;
        private IRepositoryCommitsClient _commitsClient;
        private MockCompareResult _compareResults;
        private IRepositoryContentsClient _contentsClient;
        private IDiffToolLauncher _diffTool;
        private IFileContentPersist _fileContentPersist;
        private IGitHubClient _gitHubClient;
        private string _headFileName;
        private MainWindowVm _mainWindowVm;
        private IPatchService _patchService;
        private IPullRequestsClient _prClient;
        private MockPullRequest _pullRequest;
        private IRepositoriesClient _repoClient;
        private IRepoHistoryPersist _repoHistoryPersist;
        private IIssueCommentsClient _reviewClient;

        [SetUp]
        public void SetUp()
        {
            _compareResults = new MockCompareResult();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _repoClient = Substitute.For<IRepositoriesClient>();
            _commitsClient = Substitute.For<IRepositoryCommitsClient>();
            _prClient = Substitute.For<IPullRequestsClient>();
            _contentsClient = Substitute.For<IRepositoryContentsClient>();
            _fileContentPersist = Substitute.For<IFileContentPersist>();
            _diffTool = Substitute.For<IDiffToolLauncher>();
            _patchService = Substitute.For<IPatchService>();
            _reviewClient = Substitute.For<IIssueCommentsClient>();
            _commentsBuilder = Substitute.For<ICommentsBuilder>();
            _commentsPersist = Substitute.For<ICommentsPersist>();
            _repoHistoryPersist = Substitute.For<IRepoHistoryPersist>();

            _gitHubClient.Repository.Returns(_repoClient);
            _repoClient.Commits.Returns(_commitsClient);
            _repoClient.PullRequest.Returns(_prClient);
            _repoClient.Content.Returns(_contentsClient);
            _gitHubClient.Issue.Comment.Returns(_reviewClient);

            _commitsClient.Compare(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>()
                ).Returns(Task.FromResult((CompareResult) _compareResults));

            _mainWindowVm = new MainWindowVm(_gitHubClient, _fileContentPersist,
                _diffTool, _patchService, _commentsBuilder,
                _commentsPersist, _repoHistoryPersist)
            {
                PullRequestLocator = _pullRequestLocator,
                IsUrlMode = false
            };

            _pullRequest = new MockPullRequest {Number = _pullRequestLocator.PullRequestNumber};
            _prClient.Get(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository,
                _mainWindowVm.PullRequestLocator.PullRequestNumber).Returns(Task.FromResult((PullRequest) _pullRequest));

            _baseFileName = MainWindowVm.BuildBaseFileName(_pullRequest.Base.Sha, _compareResults.File1.Filename);
            _headFileName = MainWindowVm.BuildHeadFileName(_pullRequest.Head.Sha, _compareResults.File1.Filename);

            _commentsContainer = new CommentsContainer {GeneralComments = GeneralComments};
            _commentsContainer.FileComments.Add(new FileComment
            {
                FileName = _compareResults.File1.Filename,
                Comments = Comment1,
                ReviewStatus = ReviewStatus1
            });
            _commentsContainer.FileComments.Add(new FileComment
            {
                FileName = _compareResults.File2.Filename,
                Comments = Comment2,
                ReviewStatus = ReviewStatus2
            });
            _commentsPersist.Load(Arg.Is<PullRequestLocator>(x => x.Equals(_pullRequestLocator)))
                .Returns(Task.FromResult(_commentsContainer));
        }

        [Test]
        public async void ShouldBeAbleToGetDiffsForPullRequest()
        {
            await _mainWindowVm.RetrieveDiffs();

            _prClient.Received(1)
                .Get(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository,
                    _mainWindowVm.PullRequestLocator.PullRequestNumber).IgnoreAsyncWarning();
            _commitsClient.Received(1)
                .Compare(_mainWindowVm.PullRequestLocator.Owner, _mainWindowVm.PullRequestLocator.Repository,
                    _pullRequest.Base.Sha, _pullRequest.Head.Sha).IgnoreAsyncWarning();
            var githubCommitFiles = _mainWindowVm.Diffs.Select(r => r.GitHubCommitFile);
            Assert.That(githubCommitFiles, Contains.Item(_compareResults.File1));
            Assert.That(githubCommitFiles, Contains.Item(_compareResults.File2));
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
                .Do(x => { throw new Exception(); });

            Assert.Throws<Exception>(async () => await _mainWindowVm.RetrieveDiffs());

            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public void ShouldBeInUrlModeByDefault()
        {
            Assert.True(new MainWindowVm(_gitHubClient, _fileContentPersist,
                _diffTool, _patchService, _commentsBuilder,
                _commentsPersist, _repoHistoryPersist).IsUrlMode);
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
            Assert.That(_mainWindowVm.PullRequestLocator.PullRequestNumber,
                Is.EqualTo(_pullRequestLocator.PullRequestNumber));
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

        [Test]
        public async void ShouldStoreBaseCommitAndHeadCommit()
        {
            Assert.IsNullOrEmpty(_mainWindowVm.BaseCommit);
            Assert.IsNullOrEmpty(_mainWindowVm.HeadCommit);
            await _mainWindowVm.RetrieveDiffs();
            Assert.That(_mainWindowVm.BaseCommit, Is.EqualTo(_pullRequest.Base.Sha));
            Assert.That(_mainWindowVm.HeadCommit, Is.EqualTo(_pullRequest.Head.Sha));
        }

        [Test]
        public async void CanRetieveFileContent()
        {
            MockFile1PersistFor("baseContent", _pullRequest.Base.Sha);
            MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            var basePath = _compareResults.File1.GetFilePath(_pullRequest.Base.Sha);
            var headPath = _compareResults.File1.GetFilePath(_pullRequest.Head.Sha);

            _contentsClient.Received(1).GetContents(_pullRequestLocator.Owner, _pullRequestLocator.Repository,
                basePath).IgnoreAsyncWarning();

            _contentsClient.Received(1).GetContents(_pullRequestLocator.Owner, _pullRequestLocator.Repository,
                headPath).IgnoreAsyncWarning();
        }

        [Test]
        public async void BusyStatusSetCorretly_WhenRetrieveFileContent()
        {
            MockFile1PersistFor("baseContent", _pullRequest.Base.Sha);
            MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);
            await _mainWindowVm.RetrieveDiffs();

            var updateCount = 0;
            _mainWindowVm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((MainWindowVm x) => x.IsProcessing))
                {
                    updateCount++;
                }
            };

            await _mainWindowVm.PrepareDiffContent();

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public async void BusyStatusSetCorretly_WhenFailedToGetContent()
        {
            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);
            await _mainWindowVm.RetrieveDiffs();

            _contentsClient.When(x => x.GetContents(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>())).Do(x => { throw new Exception(); });

            Assert.Throws<Exception>(async () => await _mainWindowVm.PrepareDiffContent());

            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public async void SaveToTempDir_WhenFileContentRecieved()
        {
            var baseContent = MockFile1PersistFor("baseContent", _pullRequest.Base.Sha);

            var headContent = MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            _fileContentPersist.Received(1).SaveContent(_pullRequestLocator,
                _headFileName,
                headContent.Content).IgnoreAsyncWarning();
            _fileContentPersist.Received(1).SaveContent(_pullRequestLocator,
                _baseFileName,
                baseContent.Content).IgnoreAsyncWarning();
        }

        [Test]
        public async void CanCallDiffTool()
        {
            var baseContent = MockFile1PersistFor("baseContent", _pullRequest.Base.Sha);
            var headContent = MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            const string basePath = "basepath";
            _fileContentPersist.SaveContent(Arg.Any<PullRequestLocator>(),
                Arg.Any<string>(),
                baseContent.Content).Returns(Task.FromResult(basePath));
            const string headPath = "headpath";
            _fileContentPersist.SaveContent(Arg.Any<PullRequestLocator>(),
                Arg.Any<string>(),
                headContent.Content).Returns(Task.FromResult(headPath));

            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            _diffTool.Received(1).Open(basePath, headPath);
        }

        [Test]
        public async void AbleToCachedFiles()
        {
            _fileContentPersist.ExistsInCached(Arg.Any<PullRequestLocator>(),
                _baseFileName).Returns(true);
            _fileContentPersist.ExistsInCached(Arg.Any<PullRequestLocator>(),
                _headFileName).Returns(true);
            const string cachedPath = "DummyPath";
            _fileContentPersist.GetCachedFilePath(Arg.Any<PullRequestLocator>(), Arg.Any<string>()).Returns(cachedPath);

            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            _contentsClient.DidNotReceiveWithAnyArgs().GetContents("", "", "").IgnoreAsyncWarning();
            _fileContentPersist.DidNotReceiveWithAnyArgs().SaveContent(null, "", "").IgnoreAsyncWarning();
            _fileContentPersist.Received(1).GetCachedFilePath(_pullRequestLocator, _baseFileName);
            _fileContentPersist.Received(1).GetCachedFilePath(_pullRequestLocator, _headFileName);
            _diffTool.Received(1).Open(cachedPath, cachedPath);
        }

        [Test]
        public async void GivenANewAddedFile_ShouldProvideAFakeBaseFile()
        {
            var headContent = MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            const string basePath = "basepath";
            _fileContentPersist.SaveContent(Arg.Any<PullRequestLocator>(),
                Arg.Any<string>(),
                "").Returns(Task.FromResult(basePath));

            const string headPath = "headpath";
            _fileContentPersist.SaveContent(Arg.Any<PullRequestLocator>(),
                Arg.Any<string>(),
                headContent.Content).Returns(Task.FromResult(headPath));

            _compareResults.File1.Status = GitFileStatus.New;
            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            _contentsClient.DidNotReceive().GetContents(_pullRequestLocator.Owner,
                _pullRequestLocator.Repository, _baseFileName).IgnoreAsyncWarning();
            _diffTool.Received(1).Open(basePath, headPath);
        }

        [Test]
        public async void GivenARenamedFile_ShouldCallPatchToGetBaseFile()
        {
            _patchService.RevertViaPatch(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(""));

            var headContent = MockFile1PersistFor("headContent", _pullRequest.Head.Sha);

            const string basePath = "basePath";
            _fileContentPersist.GetCachedFilePath(_pullRequestLocator, _baseFileName).Returns(basePath);

            const string headPath = "headpath";
            _fileContentPersist.SaveContent(Arg.Any<PullRequestLocator>(),
                Arg.Any<string>(),
                headContent.Content).Returns(Task.FromResult(headPath));

            _fileContentPersist.ReadContent(headPath).Returns(Task.FromResult(headContent.Content));

            _compareResults.File1.Status = GitFileStatus.Renamed;
            _mainWindowVm.SelectedDiffFile = new CommitFileVm(_compareResults.File1);

            await _mainWindowVm.RetrieveDiffs();

            await _mainWindowVm.PrepareDiffContent();

            _patchService.Received(1)
                .RevertViaPatch(headContent.Content, _compareResults.File1.Patch, basePath)
                .IgnoreAsyncWarning();

            _fileContentPersist.DidNotReceive().SaveContent(Arg.Any<PullRequestLocator>(),
                _baseFileName,
                Arg.Any<string>()).IgnoreAsyncWarning();

            _contentsClient.DidNotReceive().GetContents(_pullRequestLocator.Owner,
                _pullRequestLocator.Repository, _baseFileName).IgnoreAsyncWarning();
            _diffTool.Received(1).Open(basePath, headPath);
        }

        [Test]
        public async void TestHasComments()
        {
            await _mainWindowVm.RetrieveDiffs();

            // reset.
            foreach (var diff in _mainWindowVm.Diffs)
            {
                diff.Comments = "";
            }
            _mainWindowVm.GeneralComments = "";

            Assert.False(_mainWindowVm.HasComments());

            _mainWindowVm.Diffs.First().Comments = "my comment on file";
            Assert.True(_mainWindowVm.HasComments());

            _mainWindowVm.Diffs.First().Comments = null;
            Assert.False(_mainWindowVm.HasComments());

            _mainWindowVm.Diffs.First().Comments = "";
            Assert.False(_mainWindowVm.HasComments());

            _mainWindowVm.GeneralComments = "my general comments";
            Assert.True(_mainWindowVm.HasComments());

            _mainWindowVm.GeneralComments = null;
            Assert.False(_mainWindowVm.HasComments());

            _mainWindowVm.GeneralComments = "";
            Assert.False(_mainWindowVm.HasComments());
        }

        [Test]
        public async void TestSubmitComments()
        {
            var expectedComments = string.Join("\r\n", Comment1, Comment2, GeneralComments);

            _commentsBuilder.Build(_mainWindowVm.Diffs, _mainWindowVm.GeneralComments)
                .Returns(expectedComments);

            await _mainWindowVm.SubmitComments();

            _reviewClient.Received(1).Create(_pullRequestLocator.Owner, _pullRequestLocator.Repository,
                _pullRequestLocator.PullRequestNumber, expectedComments).IgnoreAsyncWarning();
        }

        [Test]
        public async void CanClearComments()
        {
            await _mainWindowVm.ClearComments();
            _commentsPersist.Received(1).Delete(_mainWindowVm.PullRequestLocator).IgnoreAsyncWarning();
            foreach (var diff in _mainWindowVm.Diffs)
            {
                Assert.IsNullOrEmpty(diff.Comments);
            }
            Assert.IsNullOrEmpty(_mainWindowVm.GeneralComments);
        }

        [Test]
        public async void BusyStatusSetCorretly_WhenSubmitComments()
        {
            var expectedComments = string.Join("\r\n", Comment1, Comment2, GeneralComments);
            _commentsBuilder.Build(_mainWindowVm.Diffs, _mainWindowVm.GeneralComments)
                .Returns(expectedComments);

            var updateCount = 0;
            _mainWindowVm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((MainWindowVm x) => x.IsProcessing))
                {
                    updateCount++;
                }
            };

            await _mainWindowVm.SubmitComments();

            Assert.That(updateCount, Is.EqualTo(2));
            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
#pragma warning disable 1998
        public async void BusyStatusSetCorretly_WhenFailedSubmitComments()
#pragma warning restore 1998
        {
            _commentsBuilder.Build(_mainWindowVm.Diffs, _mainWindowVm.GeneralComments)
                .Returns("");


            _reviewClient.WhenForAnyArgs(x => x.Create("", "", 1, null)).Do(x => { throw new Exception(); });

            Assert.Throws<Exception>(async () => await _mainWindowVm.SubmitComments());

            Assert.False(_mainWindowVm.IsProcessing);
        }

        [Test]
        public async void CanSaveComments()
        {
            await _mainWindowVm.SaveComments();
            _commentsPersist.Received(1)
                .Save(_pullRequestLocator, _mainWindowVm.Diffs, _mainWindowVm.GeneralComments)
                .IgnoreAsyncWarning();
        }

        [Test]
        public async void NoNeedToSave_IfNoPullRequestInfo()
        {
            _mainWindowVm.PullRequestLocator = PullRequestLocator.Empty;
            await _mainWindowVm.SaveComments();
            _commentsPersist.DidNotReceiveWithAnyArgs().Save(null, null, "").IgnoreAsyncWarning();
        }

        [Test]
        public async void CanLoadCommentsAndReviewStatus()
        {
            await _mainWindowVm.RetrieveDiffs();
            _commentsPersist.Received(1).Load(_pullRequestLocator).IgnoreAsyncWarning();
            Assert.That(_mainWindowVm.GeneralComments, Is.EqualTo(GeneralComments));
            Assert.That(_mainWindowVm.Diffs[0].Comments, Is.EqualTo(Comment1));
            Assert.That(_mainWindowVm.Diffs[0].ReviewStatus, Is.EqualTo(ReviewStatus1));
            Assert.That(_mainWindowVm.Diffs[1].Comments, Is.EqualTo(Comment2));
            Assert.That(_mainWindowVm.Diffs[1].ReviewStatus, Is.EqualTo(ReviewStatus2));
        }

        [Test]
#pragma warning disable 1998
        public async void ShouldSkipIfFileNoLongerExistsInThePR()
#pragma warning restore 1998
        {
            _commentsContainer.FileComments.Add(new FileComment
            {
                FileName = "DummyFile" + Guid.NewGuid(),
                Comments = "Comment of DummyFile"
            });
            Assert.DoesNotThrow(async () => await _mainWindowVm.RetrieveDiffs());
        }

        [Test]
        public void TestConvertUrlToPrLocator()
        {
            _mainWindowVm.PullRequestUrl = @"https://github.com/ebenzhang/ezplayer/pull/119";
            Assert.That(_mainWindowVm.PullRequestLocator.Owner, Is.EqualTo("ebenzhang"));
            Assert.That(_mainWindowVm.PullRequestLocator.Repository, Is.EqualTo("ezplayer"));
            Assert.That(_mainWindowVm.PullRequestLocator.PullRequestNumber, Is.EqualTo(119));

            _mainWindowVm.PullRequestUrl = @"";
            Assert.That(_mainWindowVm.PullRequestLocator, Is.EqualTo(PullRequestLocator.Empty));

            _mainWindowVm.PullRequestUrl = @"https://github.com/git/gitrepo/pull/19";
            Assert.That(_mainWindowVm.PullRequestLocator.Owner, Is.EqualTo("git"));
            Assert.That(_mainWindowVm.PullRequestLocator.Repository, Is.EqualTo("gitrepo"));
            Assert.That(_mainWindowVm.PullRequestLocator.PullRequestNumber, Is.EqualTo(19));

            _mainWindowVm.PullRequestUrl = @"https://github.com/git/gitrepo/";
            Assert.That(_mainWindowVm.PullRequestLocator, Is.EqualTo(PullRequestLocator.Empty));
        }

        [Test]
        public async void ShouldStoreOwnerAndRepoHistoryForAutoCompletion()
        {
            await _mainWindowVm.RetrieveDiffs();
            CollectionAssert.Contains(_mainWindowVm.RecentRepoes.Owners, _pullRequestLocator.Owner);
            CollectionAssert.Contains(_mainWindowVm.RecentRepoes.Repositories, _pullRequestLocator.Repository);
            CollectionAssert.Contains(_mainWindowVm.RecentRepoes.PullRequests, _pullRequestLocator);

            _repoHistoryPersist.Received(1)
                .Save(Arg.Is<RepoHistoryContainer>(r => r.Equals(_mainWindowVm.RecentRepoes.ToContainer())))
                .IgnoreAsyncWarning();
        }

        [Test]
        public async void HasTheAbilityToLoadRepoHistory()
        {
            var container = new RepoHistoryContainer()
            {
                Owners = new List<string> {"owner1", "owner2"},
                Repositories = new List<string> {"repo1", "repo2"}
            };
            _repoHistoryPersist.Load().Returns(Task.FromResult(container));

            await _mainWindowVm.LoadRepoHistory();

            _repoHistoryPersist.Received(1).Load().IgnoreAsyncWarning();
            CollectionAssert.AreEqual(_mainWindowVm.RecentRepoes.Owners, container.Owners);
            CollectionAssert.AreEqual(_mainWindowVm.RecentRepoes.Repositories, container.Repositories);
        }

        [Test]
        public async void ShouldSaveCommentsBeforeRetrieveDiff()
        {
            const string expectedGeneralComments = GeneralComments + "Comments Before Retrieve Diff";
            _mainWindowVm.GeneralComments = expectedGeneralComments;
            var expectedDiff = new CommitFileVm(new MockGitHubCommitFile());
            _mainWindowVm.Diffs.Add(expectedDiff);
            _mainWindowVm.Diffs[0].Comments = Comment1 + "Comments Before Retrieve Diff";

            _commentsPersist.When(r => r.Save(_pullRequestLocator, Arg.Any<ObservableCollection<CommitFileVm>>(), expectedGeneralComments)).Do(x =>
            {// have to verify in 'When Do', because the vm.Diffs changed by RetrieveDiffs when verifying via 'Recieved' method
                var diffs = x.Args()[1] as ObservableCollection<CommitFileVm>;
                Assert.That(diffs.Count, Is.EqualTo(1));
                Assert.That(diffs[0], Is.EqualTo(expectedDiff));
            });

            await _mainWindowVm.RetrieveDiffs();
        }

        private MockRepositoryContent MockFile1PersistFor(string rawContent, string sha)
        {
            var headContent = new MockRepositoryContent {EncodedContent = rawContent};
            IReadOnlyList<RepositoryContent> headContentCollection =
                new List<RepositoryContent> {headContent}.AsReadOnly();
            _contentsClient.GetContents(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string>(x => x == _compareResults.File1.GetFilePath(sha)))
                .Returns(Task.FromResult(headContentCollection));
            return headContent;
        }
    }

    internal class MockRepositoryContent : RepositoryContent
    {
        public new string EncodedContent
        {
            set { base.EncodedContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((value))); }
        }
    }

    internal class MockPullRequest : PullRequest
    {
        public MockPullRequest()
        {
            Base = new GitReference("", "", "", "1212", null, null);
            Head = new GitReference("", "", "", "asdfasdf", null, null);
        }

        public new int Number
        {
            get { return base.Number; }
            set { base.Number = value; }
        }
    }

    internal class MockGitHubCommit : GitHubCommit
    {
        public new string Sha
        {
            get { return base.Sha; }
            set { base.Sha = value; }
        }
    }

    internal class MockGitHubCommitFile : GitHubCommitFile
    {
        public new string Sha
        {
            get { return base.Sha; }
            set { base.Sha = value; }
        }

        public new string Filename
        {
            get { return base.Filename; }
            set { base.Filename = value; }
        }

        public new string Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        public new string Patch
        {
            get { return base.Patch; }
            set { base.Patch = value; }
        }
    }

    internal class MockCompareResult : CompareResult
    {
        public MockGitHubCommitFile File1 = new MockGitHubCommitFile
        {
            Sha = "e74fe8d371a5e33c4877f662e6f8ed7c0949a8b0",
            Filename = "test.xaml",
            Patch = "Patch"
        };

        public MockGitHubCommitFile File2 = new MockGitHubCommitFile
        {
            Sha = "9dc7f01526e368a64c49714c51f1d851885793ba",
            Filename = "app.xaml.cs"
        };

        public MockCompareResult()
        {
            Files = new List<GitHubCommitFile>
            {
                File1,
                File2
            };
            var mockBasCommit = new MockGitHubCommit {Sha = "ef4f2857776d06cc28acedbd023bbb33ca83d216"};
            BaseCommit = mockBasCommit;
        }
    }
}