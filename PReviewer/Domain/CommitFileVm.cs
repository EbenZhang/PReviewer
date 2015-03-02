using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Octokit;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public class CommitFileVm : ViewModelBase
    {
        private GitHubCommitFile _gitHubCommitFile;
        private string _comments;
        private ReviewStatus _reviewStatus = ReviewStatus.HasntBeenReviewed;

        public GitHubCommitFile GitHubCommitFile
        {
            get { return _gitHubCommitFile; }
            set
            {
                _gitHubCommitFile = value; 
                RaisePropertyChanged();
            }
        }

        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value; 
                RaisePropertyChanged();
            }
        }

        public ReviewStatus ReviewStatus
        {
            get
            {
                return _reviewStatus; 
            }
            set
            {
                _reviewStatus = value;
                RaisePropertyChanged();
            }
        }

        public CommitFileVm(GitHubCommitFile gitHubCommitFile)
        {
            GitHubCommitFile = gitHubCommitFile;
        }

        public bool IsTheSameAsOrgFile()
        {
            return GitHubCommitFile.Changes == 0
                   && GitHubCommitFile.Additions == 0
                   && GitHubCommitFile.Deletions == 0;
        }

        public string GetFilePath(string commitSha)
        {
            return GitHubCommitFile.GetFilePath(commitSha);
        }

        public ICommand ChangeReviewStatusCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var nextStatus = (int) (this.ReviewStatus + 1);
                    nextStatus = nextStatus%(int) ReviewStatus.Max;
                    this.ReviewStatus = (ReviewStatus) nextStatus;
                });
            }
        }
    }
}
