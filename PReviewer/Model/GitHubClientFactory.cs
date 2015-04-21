using System.Threading.Tasks;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        private IGitHubClient _client;
        private readonly object _lockClient = new object();
        public async Task<IGitHubClient> Login(string userName, string password)
        {
            var github = new GitHubClient(new ProductHeaderValue("PReviewer"))
            {
                Credentials = new Credentials(userName, password)
            };

            await github.User.Current();

            lock (_lockClient)
            {
                _client = github;
            }

            return _client;
        }

        public IGitHubClient GetClient()
        {
            lock (_lockClient)
            {
                return _client;
            }
        }
    }
}
