using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ExtendedCL;
using GalaSoft.MvvmLight;
using Octokit;
using PReviewer.Model;

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
        private string _generalComments;
        private bool _IsProcessing;
        private bool _IsUrlMode = true;
        private PullRequestLocator _PullRequestLocator = PullRequestLocator.Empty;
        private string _PullRequestUrl;
        private RecentRepo _recentRepoes = new RecentRepo();
        private CommitFileVm _SelectedDiffFile;
        public string BaseCommit;
        public string HeadCommit;

        public MainWindowVm(IGitHubClient client, IFileContentPersist fileContentPersist,
            IDiffToolLauncher diffTool,
            IPatchService patchService,
            ICommentsBuilder commentsBuilder,
            ICommentsPersist commentsPersist,
            IRepoHistoryPersist repoHistoryPersist)
        {
            Diffs = new ObservableCollection<CommitFileVm>();
            _client = client;
            _fileContentPersist = fileContentPersist;
            _diffTool = diffTool;
            _patchService = patchService;
            _reviewClient = client.Issue.Comment;
            _commentsBuilder = commentsBuilder;
            _commentsPersist = commentsPersist;
            _repoHistoryPersist = repoHistoryPersist;
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
                if (!string.IsNullOrWhiteSpace(_PullRequestUrl))
                {
                    try
                    {
                        PullRequestLocator = PullRequestLocator.FromUrl(_PullRequestUrl);
                    }
                    catch
                    {
                        PullRequestLocator = PullRequestLocator.Empty;
                    }
                }
                else
                {
                    PullRequestLocator = PullRequestLocator.Empty;
                }
            }
        }

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
            IsProcessing = true;
            try
            {
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
                BaseCommit = pr.Base.Sha;
                HeadCommit = pr.Head.Sha;

                var comments = await _commentsPersist.Load(PullRequestLocator);
                GeneralComments = comments.GeneralComments;

                foreach (var fileComment in comments.FileComments)
                {
                    var file = Diffs.SingleOrDefault(r => r.GitHubCommitFile.Filename == fileComment.FileName);
                    if (file == null) continue;
                    file.Comments = fileComment.Comments;
                    file.ReviewStatus = fileComment.ReviewStatus;
                }

                await RecentRepoes.Save(PullRequestLocator, _repoHistoryPersist);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public async Task PrepareDiffContent()
        {
            try
            {
                IsProcessing = true;
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
            finally
            {
                IsProcessing = false;
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
            try
            {
                IsProcessing = true;
                var comments = _commentsBuilder.Build(Diffs, GeneralComments);
                await _reviewClient.Create(_PullRequestLocator.Owner,
                    _PullRequestLocator.Repository,
                    _PullRequestLocator.PullRequestNumber, comments);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public async Task SaveComments()
        {
            try
            {
                if (PullRequestLocator == PullRequestLocator.Empty)
                {
                    return;
                }
                IsProcessing = true;
                await _commentsPersist.Save(_PullRequestLocator, Diffs, GeneralComments);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public async Task LoadRepoHistory()
        {
            try
            {
                IsProcessing = true;
                var historyContainer = await _repoHistoryPersist.Load();
                RecentRepoes.Owners.Assign(historyContainer.Owners);
                RecentRepoes.Repositories.Assign(historyContainer.Repositories);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
