using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ExtendedCL;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using Octokit;
using PReviewer.Model;
using PReviewer.Service;

namespace PReviewer.Domain
{
    public class MainWindowVm : ViewModelBase
    {
        private readonly IGitHubClient _client;
        private readonly ICommentsBuilder _commentsBuilder;
        private readonly ICommentsPersist _commentsPersist;
        private readonly IDiffToolLauncher _diffTool;
        private readonly IFileContentPersist _fileContentPersist;
        private readonly IPatchService _patchService;
        private readonly IRepoHistoryPersist _repoHistoryPersist;
        private readonly IIssueCommentsClient _reviewClient;
        private readonly IBackgroundTaskRunner _backgroundTaskRunner;
        private string _generalComments;
        private bool _IsProcessing;
        private bool _IsUrlMode = true;
        private PullRequestLocator _PullRequestLocator = PullRequestLocator.Empty;
        private string _PullRequestUrl;
        private RecentRepo _recentRepoes = new RecentRepo();
        private CommitFileVm _SelectedDiffFile;

        public string BaseCommit
        {
            get { return _baseCommit; }
            set
            {
                _baseCommit = value;
                RaisePropertyChanged();
            }
        }

        public string HeadCommit
        {
            get { return _headCommit; }
            set
            {
                _headCommit = value;
                RaisePropertyChanged();
            }
        }

        private string _prTitle;
        private string _prDescription;
        private string _baseCommit;
        private string _headCommit;

        public MainWindowVm(IGitHubClient client, IFileContentPersist fileContentPersist,
            IDiffToolLauncher diffTool,
            IPatchService patchService,
            ICommentsBuilder commentsBuilder,
            ICommentsPersist commentsPersist,
            IRepoHistoryPersist repoHistoryPersist,
            IBackgroundTaskRunner backgroundTaskRunner)
        {
            Diffs = new ObservableCollection<CommitFileVm>();
            Commits = new ObservableCollection<CommitVm>();
            _client = client;
            _fileContentPersist = fileContentPersist;
            _diffTool = diffTool;
            _patchService = patchService;
            _reviewClient = client.Issue.Comment;
            _commentsBuilder = commentsBuilder;
            _commentsPersist = commentsPersist;
            _repoHistoryPersist = repoHistoryPersist;
            _backgroundTaskRunner = backgroundTaskRunner;
        }

        public ObservableCollection<CommitFileVm> Diffs { get; set; }

        public bool IsProcessing
        {
            get { return _IsProcessing; }
            set
            {
                _IsProcessing = value;
                RaisePropertyChanged();
            }
        }

        public string PullRequestUrl
        {
            get { return _PullRequestUrl; }
            set
            {
                _PullRequestUrl = value;
                RaisePropertyChanged();
            }
        }

        private PullRequestLocator _prePullRequestLocator = Domain.PullRequestLocator.Empty;
        public PullRequestLocator PullRequestLocator
        {
            get { return _PullRequestLocator; }
            set
            {
                _PullRequestLocator = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Indicates if pull request Url selected.
        ///     In this mode, we need to parse the Url to get the
        ///     - Owner
        ///     - Repo
        ///     - PR number.
        /// </summary>
        public bool IsUrlMode
        {
            get { return _IsUrlMode; }
            set
            {
                _IsUrlMode = value;
                RaisePropertyChanged();
            }
        }

        public CommitFileVm SelectedDiffFile
        {
            get { return _SelectedDiffFile; }
            set
            {
                _SelectedDiffFile = value;
                RaisePropertyChanged();
            }
        }

        public string GeneralComments
        {
            get { return _generalComments; }
            set
            {
                _generalComments = value;
                RaisePropertyChanged();
            }
        }

        public RecentRepo RecentRepoes
        {
            get { return _recentRepoes; }
            set
            {
                _recentRepoes = value;
                RaisePropertyChanged();
            }
        }

        public async Task RetrieveDiffs()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                await SaveCommentsWithoutChangeBusyStatus(_prePullRequestLocator);
                _prePullRequestLocator = PullRequestLocator;
                if (IsUrlMode)
                {
                    try
                    {
                        PullRequestLocator.UpdateWith(PullRequestUrl);
                    }
                    catch (Exception ex)
                    {
                        throw new UriFormatException(ex.ToString());
                    }
                }
                else
                {
                    PullRequestUrl = PullRequestLocator.ToUrl();
                }
                var repo = _client.Repository;
                var pr =
                    await
                        repo.PullRequest.Get(PullRequestLocator.Owner, PullRequestLocator.Repository,
                            PullRequestLocator.PullRequestNumber);
                var commitsClient = repo.Commits;
                var compareResult =
                    await
                        commitsClient.Compare(PullRequestLocator.Owner, PullRequestLocator.Repository, pr.Base.Sha,
                            pr.Head.Sha);
                Diffs.Assign(compareResult.Files.Select(r => new CommitFileVm(r)));
                Commits.Clear();
                Commits.Add(new CommitVm(pr.Base.Label, pr.Base.Sha));
                Commits.Add(new CommitVm(pr.Head.Label, pr.Head.Sha));
                BaseCommit = pr.Base.Sha;
                HeadCommit = pr.Head.Sha;

                _backgroundTaskRunner.RunInBackground(() => RetrieveCommits(repo.PullRequest,
                    _PullRequestLocator, new CommitsCombiner(Commits)));

                PrTitle = pr.Title;
                PrDescription = string.IsNullOrWhiteSpace(pr.Body) ? DefaultPrDescription : pr.Body;

                await ReloadComments();

                await RecentRepoes.Save(PullRequestLocator, _repoHistoryPersist);
            }
        }

        private static void RetrieveCommits(IPullRequestsClient prClient,
            PullRequestLocator prLocator,
            CommitsCombiner combiner)
        {
            var commits = prClient.Commits(prLocator.Owner, prLocator.Repository,
                prLocator.PullRequestNumber).Result;

            DispatcherHelper.RunAsync(() =>
            {
                combiner.Add(commits);
            });
        }

        public async Task PrepareDiffContent()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                var headFileName = BuildHeadFileName(HeadCommit, SelectedDiffFile.GitHubCommitFile.Filename);
                var headPath = "";
                string contentOfHead = null;
                if (!_fileContentPersist.ExistsInCached(_PullRequestLocator, headFileName))
                {
                    var collectionOfContentOfHead =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GitHubCommitFile.GetFilePath(HeadCommit));

                    contentOfHead = collectionOfContentOfHead.First().Content;
                    headPath = await SaveToFile(headFileName, contentOfHead);
                }
                else
                {
                    headPath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, headFileName);
                }

                var baseFileName = BuildBaseFileName(BaseCommit, SelectedDiffFile.GitHubCommitFile.Filename);
                var basePath = "";
                if (_fileContentPersist.ExistsInCached(_PullRequestLocator, baseFileName))
                {
                    basePath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, baseFileName);
                }
                else if (SelectedDiffFile.GitHubCommitFile.Status == GitFileStatus.Renamed)
                {
                    if (contentOfHead == null)
                    {
                        contentOfHead = await _fileContentPersist.ReadContent(headPath);
                    }

                    basePath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, baseFileName);

                    await _patchService.RevertViaPatch(contentOfHead, SelectedDiffFile.GitHubCommitFile.Patch, basePath);
                }
                else if (SelectedDiffFile.GitHubCommitFile.Status == GitFileStatus.New)
                {
                    basePath = await SaveToFile(baseFileName, "");
                }
                else
                {
                    var contentOfBase =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GitHubCommitFile.GetFilePath(BaseCommit));

                    basePath = await SaveToFile(baseFileName, contentOfBase.First().Content);
                }

                _diffTool.Open(basePath, headPath);
            }
        }

        public static string BuildHeadFileName(string headCommit, string orgFileName)
        {
            return headCommit + "/Head/" + orgFileName;
        }

        public static string BuildBaseFileName(string baseCommit, string orgFileName)
        {
            return baseCommit + "/Base/" + orgFileName;
        }

        public async Task<string> SaveToFile(string fileName, string content)
        {
            return await _fileContentPersist.SaveContent(PullRequestLocator, fileName, content);
        }

        public bool HasComments()
        {
            if (!string.IsNullOrWhiteSpace(GeneralComments))
            {
                return true;
            }
            var hasFileComment = Diffs.Any(r => !string.IsNullOrWhiteSpace(r.Comments));
            return hasFileComment;
        }

        public async Task SubmitComments()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                var comments = _commentsBuilder.Build(Diffs, GeneralComments);
                await _reviewClient.Create(_PullRequestLocator.Owner,
                    _PullRequestLocator.Repository,
                    _PullRequestLocator.PullRequestNumber, comments);
            }
        }

        public async Task SaveComments()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                await SaveCommentsWithoutChangeBusyStatus(PullRequestLocator);
            }
        }

        private async Task SaveCommentsWithoutChangeBusyStatus(PullRequestLocator request)
        {
            if (request == null || !request.IsValid())
            {
                return;
            }
            await _commentsPersist.Save(_PullRequestLocator, Diffs, GeneralComments);
        }

        public async Task LoadRepoHistory()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                var historyContainer = await _repoHistoryPersist.Load();
                RecentRepoes.From(historyContainer);

                PullRequestLocator = RecentRepoes.PullRequests.LastOrDefault();
                if (PullRequestLocator != null)
                {
                    PullRequestUrl = PullRequestLocator.ToUrl();
                }
            }
        }

        public async Task ClearComments()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                await _commentsPersist.Delete(PullRequestLocator);
                foreach (var diff in Diffs)
                {
                    diff.Comments = "";
                }
                GeneralComments = "";
            }
        }

        public string PrTitle
        {
            get { return _prTitle; }
            set
            {
                _prTitle = value;
                RaisePropertyChanged();
            }
        }

        public string PrDescription
        {
            get { return _prDescription; }
            set
            {
                if(_prDescription != value)
                {
                    _prDescription = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<CommitVm> Commits { get; set; }

        public ICommand ChangeCommitRangeCmd
        {
            get
            {
                return new RelayCommand(ChangeCommitRange);
            }
        }

        private async void ChangeCommitRange()
        {
            using (new ScopeDisposer(() => IsProcessing = true, () => IsProcessing = false))
            {
                await SaveCommentsWithoutChangeBusyStatus(PullRequestLocator);

                var repo = _client.Repository;
                var commitsClient = repo.Commits;
                var compareResult =
                    await
                        commitsClient.Compare(PullRequestLocator.Owner, PullRequestLocator.Repository, BaseCommit,
                            HeadCommit);

                Diffs.Assign(compareResult.Files.Select(r => new CommitFileVm(r)));

                await ReloadComments();
            }
        }

        private async Task ReloadComments()
        {
            var comments = await _commentsPersist.Load(PullRequestLocator);
            GeneralComments = comments.GeneralComments;

            foreach (var fileComment in comments.FileComments)
            {
                var file = Diffs.SingleOrDefault(r => r.GitHubCommitFile.Filename == fileComment.FileName);
                if (file == null) continue;
                file.Comments = fileComment.Comments;
                file.ReviewStatus = fileComment.ReviewStatus;
            }
        }

        public static readonly string DefaultPrDescription = "## The guy is too lazy to leave anything here.";
    }
}
