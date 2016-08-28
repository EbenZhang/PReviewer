using Octokit;

namespace PReviewer.Core
{
    public static class GitHubCommitFileEx
    {
        public static string GetFilePath(this GitHubCommitFile file, string commitSha)
        {
            return file.Filename + "?ref=" + commitSha;
        }
    }
}
