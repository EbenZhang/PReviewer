using System;
using System.IO;
using System.Threading.Tasks;
using ExtendedCL;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class FileContentPersist : IFileContentPersist
    {
        public async Task<string> SaveContent(PullRequestLocator prInfo, string fileName, string content)
        {
            var filePath = GetFilePath(prInfo, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync(content);
                }
            }
            return filePath;
        }

        private static string GetRootDir()
        {
            var rootDir = Path.Combine(PathHelper.ProcessAppDir, "cached");
            if (!Directory.Exists(rootDir))
            {
                Directory.CreateDirectory(rootDir);
            }
            return rootDir;
        }

        public bool ExistsInCached(PullRequestLocator prInfo, string fileName)
        {
            var filePath = GetFilePath(prInfo, fileName);
            return File.Exists(filePath);
        }

        public string GetCachedFilePath(PullRequestLocator prInfo, string fileName)
        {
            var filePath = GetFilePath(prInfo, fileName);
            return filePath;
        }

        public async Task<string> ReadContent(string headPath)
        {
            using (var stream = File.OpenRead(headPath))
            {
                using (var sr = new StreamReader(stream))
                {
                    return await sr.ReadToEndAsync();
                }
            }
        }

        private static string GetFilePath(PullRequestLocator prInfo, string fileName)
        {
            var prDir = GetPullRequestDir(prInfo);
            var filePath = Path.Combine(prDir, fileName);
            return filePath;
        }

        public static string GetPullRequestDir(PullRequestLocator prInfo)
        {
            var prDir = Path.Combine(GetRootDir(), prInfo.Owner, prInfo.Repository, "PR" + prInfo.PullRequestNumber);
            if (!Directory.Exists(prDir))
            {
                Directory.CreateDirectory(prDir);
            }
            return prDir;
        }
    }
}