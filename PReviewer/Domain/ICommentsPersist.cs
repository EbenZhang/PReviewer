using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public interface ICommentsPersist
    {
        Task Save(PullRequestLocator prInfo, IEnumerable<CommitFileVm> diffs, string generalComments);
        Task<CommentsContainer> Load(PullRequestLocator prInfo);
        Task Delete(PullRequestLocator pullRequestLocator);
    }
}
