using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PReviewer.Domain
{
    public interface IFileContentPersist
    {
        Task<string> SaveContent(PullRequestLocator prInfo, string fileName, string content);
        bool ExistsInCached(PullRequestLocator pullRequestLocator, string fileName);
        string GetCachedFilePath(PullRequestLocator pullRequestLocator, string fileName);
        Task<string> ReadContent(string headPath);
    }
}
