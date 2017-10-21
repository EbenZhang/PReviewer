using System;
using System.IO;
using System.Threading.Tasks;
using PReviewer.Core.VcsAbstraction;

namespace PReviewer.Core
{
	public class DiffContentFetcher
    {
        private readonly PullRequestLocator _pullRequestLocator;
        private readonly IFileContentPersist _fileContentPersist;
        private readonly IPatchService _patchService;
		private readonly IFileContentRetriever _fileContentRetriever;

        public DiffContentFetcher(PullRequestLocator pullRequestLocator,
            IFileContentPersist fileContentPersist,
			IPatchService patchService, IFileContentRetriever fileContentRetriever)
        {
            _pullRequestLocator = pullRequestLocator;
            _fileContentPersist = fileContentPersist;
            _patchService = patchService;
			_fileContentRetriever = fileContentRetriever;
		}

        public async Task<Tuple<string, string>> FetchDiffContent(ICommitFile diffFile, 
            string headCommit, string baseCommit)
        {
            var headFileName = BuildHeadFileName(headCommit, diffFile.Filename);
            var headPath = "";
            string contentOfHead = null;

            if (diffFile.Status == FileStatus.Removed)
            {
                headPath = await SaveToFile(headFileName, "");
            }
            else if (!_fileContentPersist.ExistsInCached(_pullRequestLocator, headFileName))
            {
                contentOfHead = await _fileContentRetriever.GetContent(_pullRequestLocator, diffFile.GetFilePath(headCommit));
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
            else if (diffFile.Status == FileStatus.Renamed)
            {
                if (contentOfHead == null)
                {
                    contentOfHead = await _fileContentPersist.ReadContent(headPath);
                }

                basePath = _fileContentPersist.GetCachedFilePath(_pullRequestLocator, baseFileName);

                await _patchService.RevertViaPatch(contentOfHead, diffFile.Patch, basePath);
            }
            else if (diffFile.Status == FileStatus.New)
            {
                basePath = await SaveToFile(baseFileName, "");
            }
            else
            {
				var contentOfBase = await _fileContentRetriever.GetContent(_pullRequestLocator, diffFile.GetFilePath(baseCommit));
                basePath = await SaveToFile(baseFileName, contentOfBase);
            }
            return new Tuple<string, string>(basePath, headPath);
        }

        public static string BuildHeadFileName(string headCommit, string orgFileName)
        {
            return "Head/" + Path.GetFileName(orgFileName);
        }

        public static string BuildBaseFileName(string baseCommit, string orgFileName)
        {
            return "Base/" + Path.GetFileName(orgFileName);
        }

        public async Task<string> SaveToFile(string fileName, string content)
        {
            return await _fileContentPersist.SaveContent(_pullRequestLocator, fileName, content);
        }
    }
}
