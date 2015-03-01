using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
        private readonly IFileContentPersist _fileContentPersist;
        private readonly IDiffToolLauncher _diffTool;
        private readonly IPatchService _patchService;
        private bool _IsProcessing;
        private string _PullRequestUrl;
        private PullRequestLocator _PullRequestLocator = new PullRequestLocator();
        private bool _IsUrlMode = true;

        public MainWindowVm(IGitHubClient client, 
            IFileContentPersist fileContentPersist,
            IDiffToolLauncher diffTool,
            IPatchService patchService)
        {
            Diffs = new ObservableCollection<GitHubCommitFile>();
            _client = client;
            _fileContentPersist = fileContentPersist;
            _diffTool = diffTool;
            _patchService = patchService;
        }

        public ObservableCollection<GitHubCommitFile> Diffs { get; set; }

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

        public PullRequestLocator PullRequestLocator
        {
            get
            {
                return _PullRequestLocator;
            }
            set
            {
                _PullRequestLocator = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Indicates if pull request Url selected.
        /// In this mode, we need to parse the Url to get the
        ///  - Owner
        ///  - Repo
        ///  - PR number.
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

        public GitHubCommitFile SelectedDiffFile
        {
            get { return _SelectedDiffFile; }
            set
            {
                _SelectedDiffFile = value; 
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
                var pr = await repo.PullRequest.Get(PullRequestLocator.Owner, PullRequestLocator.Repository, PullRequestLocator.PullRequestNumber);
                var commitsClient = repo.Commits;
                var compareResult = await commitsClient.Compare(PullRequestLocator.Owner, PullRequestLocator.Repository, pr.Base.Sha, pr.Head.Sha);
                Diffs.Assign(compareResult.Files);
                BaseCommit = pr.Base.Sha;
                HeadCommit = pr.Head.Sha;
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public string BaseCommit = null;
        public string HeadCommit = null;
        private GitHubCommitFile _SelectedDiffFile;

        public async Task PrepareDiffContent()
        {
            try
            {
                IsProcessing = true;
                var headFileName = BuildHeadFileName(HeadCommit, SelectedDiffFile.Filename);
                var headPath = "";
                string contentOfHead = null;
                if (!_fileContentPersist.ExistsInCached(_PullRequestLocator, headFileName))
                {
                    var collectionOfContentOfHead =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GetFilePath(HeadCommit));

                    contentOfHead = collectionOfContentOfHead.First().Content;
                    headPath = await SaveToFile(headFileName, contentOfHead);
                }
                else
                {
                    headPath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, headFileName);
                }
                
                var baseFileName = BuildBaseFileName(BaseCommit, SelectedDiffFile.Filename);
                var basePath = "";
                if (_fileContentPersist.ExistsInCached(_PullRequestLocator, baseFileName))
                {
                    basePath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, baseFileName);
                }
                else if (SelectedDiffFile.Status == GitFileStatus.Renamed)
                {
                    if (contentOfHead == null)
                    {
                        contentOfHead = await _fileContentPersist.ReadContent(headPath);
                    }

                    basePath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, baseFileName);

                    await _patchService.RevertViaPatch(contentOfHead, SelectedDiffFile.Patch, basePath);
                }
                else if (SelectedDiffFile.Status == GitFileStatus.New)
                {
                    basePath = await SaveToFile(baseFileName, "");
                }
                else
                {
                    var contentOfBase =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GetFilePath(BaseCommit));
                    
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
    }
}
