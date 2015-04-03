using System.IO;
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
        private string _fileName;

        public GitHubCommitFile GitHubCommitFile
        {
            get { return _gitHubCommitFile; }
            set
            {
                _gitHubCommitFile = value; 
                RaisePropertyChanged();
                FileName = Path.GetFileName(_gitHubCommitFile.Filename);
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

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value; 
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
