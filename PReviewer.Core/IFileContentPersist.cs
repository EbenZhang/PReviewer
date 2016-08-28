using System.Threading.Tasks;

namespace PReviewer.Core
{
    public interface IFileContentPersist
    {
        Task<string> SaveContent(PullRequestLocator prInfo, string fileName, string content);
        bool ExistsInCached(PullRequestLocator pullRequestLocator, string fileName);
        string GetCachedFilePath(PullRequestLocator pullRequestLocator, string fileName);
        Task<string> ReadContent(string headPath);
    }
}
