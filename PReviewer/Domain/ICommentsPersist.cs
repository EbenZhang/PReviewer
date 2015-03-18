using System.Threading.Tasks;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public interface ICommentsPersist
    {
        Task Save(PullRequestLocator prInfo, CommentsContainer container);
        Task<CommentsContainer> Load(PullRequestLocator prInfo);
        Task Delete(PullRequestLocator pullRequestLocator);
    }
}
