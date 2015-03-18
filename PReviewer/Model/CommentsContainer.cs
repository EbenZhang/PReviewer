using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using PReviewer.Domain;

namespace PReviewer.Model
{
    [Serializable]
    public class CommentsContainer : IEquatable<CommentsContainer>
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
                ret.FileComments.Add(ConvertFrom(diff));
            }
            ret.GeneralComments = generalComments;
            return ret;
        }

        public void AddComments(List<CommitFileVm> commentsInCommitRange, string newGeneralComments)
        {
            commentsInCommitRange.ForEach(x =>
            {
                if (FileComments.All(r => r.Comments != x.GitHubCommitFile.Filename))
                {
                    FileComments.Add(ConvertFrom(x));
                }
            });

            GeneralComments = newGeneralComments;
        }

        public static FileComment ConvertFrom(CommitFileVm vm)
        {
            return new FileComment
            {
                Comments = vm.Comments,
                FileName = vm.GitHubCommitFile.Filename,
                ReviewStatus = vm.ReviewStatus
            };
        }

        public bool Equals(CommentsContainer other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GeneralComments != other.GeneralComments)
            {
                return false;
            }

            foreach (var c in FileComments)
            {
                var match = other.FileComments.FirstOrDefault(x => x.FileName == c.FileName);
                if (match == null)
                {
                    return false;
                }
                if (match.Comments != c.Comments)
                {
                    return false;
                }
            }
            return true;
        }
    }
}