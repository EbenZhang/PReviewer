using Octokit;

namespace PReviewer.Test
{
    internal class MockGitHubCommit : GitHubCommit
    {
        public new string Sha
        {
            get { return base.Sha; }
            set { base.Sha = value; }
        }
    }
}