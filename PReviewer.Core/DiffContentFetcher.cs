using System;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace PReviewer.Core
{
    public class DiffContentFetcher
    {
        private readonly PullRequestLocator _pullRequestLocator;
        private readonly IFileContentPersist _fileContentPersist;
        private readonly IGitHubClient _client;
        private readonly IPatchService _patchService;

        public DiffContentFetcher(PullRequestLocator pullRequestLocator,
            IFileContentPersist fileContentPersist, IGitHubClient client, IPatchService patchService)
        {
            _pullRequestLocator = pullRequestLocator;
            _fileContentPersist = fileContentPersist;
            _client = client;
            _patchService = patchService;
        }

        public async Task<Tuple<string, string>> FetchDiffContent(GitHubCommitFile diffFile, 
            string headCommit, string baseCommit)
        {
            var headFileName = BuildHeadFileName(headCommit, diffFile.Filename);
            var headPath = "";
            string contentOfHead = null;

            if (diffFile.Status == GitFileStatus.Removed)
            {
                headPath = await SaveToFile(headFileName, "");
            }
            else if (!_fileContentPersist.ExistsInCached(_pullRequestLocator, headFileName))
            {
                var collectionOfContentOfHead =
                    await
                        _client.Repository.Content.GetAllContents(_pullRequestLocator.Owner,
                            _pullRequestLocator.Repository,
                            diffFile.GetFilePath(headCommit));

                contentOfHead = collectionOfContentOfHead.First().Content;
                headPath = await SaveToFile(headFileName, contentOfHead);
            }
            else
            {
                headPath = _fileContentPersist.GetCachedFilePath(_pullRequestLocator, headFileName);
            }

            var baseFileName = BuildBaseFileName(baseCommit, diffFile.Filename);
            var basePath = "";
            if (_fileContentPersist.ExistsInCached(_pullRequestLocator, baseFileName))
            {
                basePath = _fileContentPersist.GetCachedFilePath(_pullRequestLocator, baseFileName);
            }
            else if (diffFile.Status == GitFileStatus.Renamed)
            {
                if (contentOfHead == null)
                {
                    contentOfHead = await _fileContentPersist.ReadContent(headPath);
                }

                basePath = _fileContentPersist.GetCachedFilePath(_pullRequestLocator, baseFileName);

                await _patchService.RevertViaPatch(contentOfHead, diffFile.Patch, basePath);
            }
            else if (diffFile.Status == GitFileStatus.New)
            {
                basePath = await SaveToFile(baseFileName, "");
            }
            else
            {
                var contentOfBase =
                    await
                        _client.Repository.Content.GetAllContents(_pullRequestLocator.Owner,
                            _pullRequestLocator.Repository,
                            diffFile.GetFilePath(baseCommit));

                basePath = await SaveToFile(baseFileName, contentOfBase.First().Content);
            }
            return new Tuple<string, string>(basePath, headPath);
        }

        public static string BuildHeadFileName(string headCommit, string orgFileName)
        {
            return headCommit + "/Head/" + orgFileName;
        }

        public static string BuildBaseFileName(string baseCommit, string orgFileName)
        {
            return baseCommit + "/Base/" + orgFileName;
        }

        public async Task<string> SaveToFile(string fileName, string content)
        {
            return await _fileContentPersist.SaveContent(_pullRequestLocator, fileName, content);
        }
    }
}
