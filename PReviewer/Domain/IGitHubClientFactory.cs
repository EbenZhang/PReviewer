using System.Threading.Tasks;
using Octokit;

namespace PReviewer.Domain
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> Login(string userName, string password);
        IGitHubClient GetClient();
    }
}
