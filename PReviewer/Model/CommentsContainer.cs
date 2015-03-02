using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PReviewer.Domain;

namespace PReviewer.Model
{
    [Serializable]
    public class CommentsContainer
    {
        public CommentsContainer()
        {
            FileComments = new List<FileComment>();
        }
        public string GeneralComments { get; set; }
        public List<FileComment> FileComments { get; set; }

        public static CommentsContainer From(IEnumerable<CommitFileVm> diffs, string generalComments)
        {
            var ret = new CommentsContainer();

            foreach (var diff in diffs)
            {
                ret.FileComments.Add(new FileComment()
                {
                    FileName = diff.GitHubCommitFile.Filename,
                    Comments = diff.Comments,
                    ReviewStatus = diff.ReviewStatus,
                });
            }
            ret.GeneralComments = generalComments;
            return ret;
        }
    }
}