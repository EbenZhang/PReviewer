using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Octokit;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public class CommitFileVm : ViewModelBase
    {
        private GitHubCommitFile _gitHubCommitFile;
        private string _comments;

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
    }
}
