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
        private bool _IsProcessing;
        private string _PullRequestUrl;
        private PullRequestLocator _PullRequestLocator = new PullRequestLocator();
        private bool _IsUrlMode = true;

        public MainWindowVm(IGitHubClient client, IFileContentPersist fileContentPersist, IDiffToolLauncher diffTool)
        {
            Diffs = new ObservableCollection<GitHubCommitFile>();
            _client = client;
            _fileContentPersist = fileContentPersist;
            _diffTool = diffTool;
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
                if (!_fileContentPersist.ExistsInCached(_PullRequestLocator, headFileName))
                {
                    var contentOfHead =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GetFilePath(HeadCommit));


                    headPath = await SaveToFile(headFileName, contentOfHead.First().Content);
                }
                else
                {
                    headPath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, headFileName);
                }
                
                var baseFileName = BuildBaseFileName(BaseCommit, SelectedDiffFile.Filename);
                var basePath = "";
                if (SelectedDiffFile.Status == GitFileStatus.New)
                {
                    basePath = await SaveToFile(baseFileName, "");
                }
                else if (!_fileContentPersist.ExistsInCached(_PullRequestLocator, baseFileName))
                {
                    var contentOfBase =
                        await
                            _client.Repository.Content.GetContents(PullRequestLocator.Owner,
                                PullRequestLocator.Repository,
                                _SelectedDiffFile.GetFilePath(BaseCommit));
                    
                    basePath = await SaveToFile(baseFileName, contentOfBase.First().Content);
                }
                else
                {
                    basePath = _fileContentPersist.GetCachedFilePath(_PullRequestLocator, baseFileName);
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
