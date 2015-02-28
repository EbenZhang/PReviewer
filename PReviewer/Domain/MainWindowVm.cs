using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtendedCL;
using GalaSoft.MvvmLight;
using Octokit;

namespace PReviewer.Domain
{
    public class MainWindowVm : ViewModelBase
    {
        private readonly IGitHubClient _client;
        private bool _IsProcessing;
        private string _PullRequestUrl;
        private PullRequestLocator _PullRequestLocator = new PullRequestLocator();

        public MainWindowVm(IGitHubClient client)
        {
            Diffs = new ObservableCollection<GitHubCommitFile>();
            _client = client;
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

        public async Task RetrieveDiffs()
        {
            IsProcessing = true;
            try
            {
                var repo = _client.Repository;
                var pr = await repo.PullRequest.Get(PullRequestLocator.Owner, PullRequestLocator.Repository, PullRequestLocator.PullRequestNumber);
                var commitsClient = repo.Commits;
                var compareResult = await commitsClient.Compare(PullRequestLocator.Owner, PullRequestLocator.Repository, pr.Base.Sha, pr.Head.Sha);
                Diffs.Assign(compareResult.Files);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
