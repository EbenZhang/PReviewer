using System.Threading.Tasks;
using Octokit;

namespace PReviewer.Domain
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> GetClient(string userName, string password);
    }
}
