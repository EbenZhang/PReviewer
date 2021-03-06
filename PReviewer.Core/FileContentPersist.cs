﻿using System;
using System.IO;
using System.Threading.Tasks;
using ExtendedCL;
using Nicologies;

namespace PReviewer.Core
{
    public class FailedToSaveContent : Exception
    {
        public FailedToSaveContent(Exception ex) : base(ex.Message, ex)
        {
            
        }
    }
    public class FileContentPersist : IFileContentPersist
    {
        public async Task<string> SaveContent(PullRequestLocator prInfo, string fileName, string content)
        {
            try
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
            catch (Exception ex)
            {
                throw new FailedToSaveContent(ex);
            }
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
            return false;
        }

        public string GetCachedFilePath(PullRequestLocator prInfo, string fileName)
        {
            var filePath = GetFilePath(prInfo, fileName);            return filePath;
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
            var fileDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }
            return filePath;
        }

        public static string GetPullRequestDir(PullRequestLocator prInfo)
        {
            var prDir = Path.Combine(Path.GetTempPath(), "PR" + prInfo.PullRequestNumber);
            if (!Directory.Exists(prDir))
            {
                Directory.CreateDirectory(prDir);
            }
            return prDir;
        }
    }
}