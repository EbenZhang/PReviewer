using System;
using System.Threading.Tasks;
using Octokit;

namespace PReviewer.Domain
{
    public interface IGitHubClientFactory
    {
        Task<Tuple<IGitHubClient, string>> RecreateClient(string clientId, string clientSecret, string authCode);
        void CreateClient(string token);
        IGitHubClient GetClient();
    }
}
