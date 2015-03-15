using Octokit;

namespace PReviewer.Test
{
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
}