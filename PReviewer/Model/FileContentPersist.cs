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
            var rootDir = Path.Combine(PathHelper.ProcessAppDir, "cached");
            if (!Directory.Exists(rootDir))
            {
                Directory.CreateDirectory(rootDir);
            }

            var filePath = GetFilePath(prInfo, fileName, rootDir);

            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    await streamWriter.WriteAsync(content);
                }
            }
            return filePath;
        }

        private static string GetFilePath(PullRequestLocator prInfo, string fileName, string rootDir)
        {
            var prDir = Path.Combine(prInfo.Owner, prInfo.Repository, "PR" + prInfo.PullRequestNumber);
            var filePath = Path.Combine(rootDir, prDir, fileName);
            return filePath;
        }
    }
}