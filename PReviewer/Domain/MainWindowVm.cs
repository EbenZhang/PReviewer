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
        private string _Repository;
        private string _Owner;
        private int _PullRequestNumber;
        private bool _IsProcessing;
        private string _PullRequestUrl;

        public MainWindowVm(IGitHubClient client)
        {
            Diffs = new ObservableCollection<GitHubCommitFile>();
            _client = client;
        }

        public string Repository
        {
            get { return _Repository; }
            set
            {
                _Repository = value;
                RaisePropertyChanged();
            }
        }

        public string Owner
        {
            get { return _Owner; }
            set
            {
                _Owner = value;
                RaisePropertyChanged();
            }
        }

        public int PullRequestNumber
        {
            get { return _PullRequestNumber; }
            set
            {
                _PullRequestNumber = value; 
                RaisePropertyChanged();
            }
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

        public async Task RetrieveDiffs()
        {
            IsProcessing = true;
            try
            {
                var repo = _client.Repository;
                var pr = await repo.PullRequest.Get(Owner, Repository, PullRequestNumber);
                var commitsClient = repo.Commits;
                var compareResult = await commitsClient.Compare(Owner, Repository, pr.Base.Sha, pr.Head.Sha);
                Diffs.Assign(compareResult.Files);
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
