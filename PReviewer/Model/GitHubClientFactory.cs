using System;
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

        public async Task<Tuple<IGitHubClient, string>> RecreateClient(string clientId, string clientSecret, string code)
        {
            var github = new GitHubClient(new ProductHeaderValue("PReviewer"));

            var tokenTask = await github.Oauth.CreateAccessToken(new OauthTokenRequest(clientId, clientSecret, code));
            
            var token = tokenTask.AccessToken;
            if (token == null)
            {
                return null;
            }
            github.Credentials = new Credentials(token);

            lock (_lockClient)
            {
                _client = github;
            }

            return new Tuple<IGitHubClient, string>(_client, token);
        }

        public void CreateClient(string token)
        {
            var github = new GitHubClient(new ProductHeaderValue("PReviewer"));
            github.Credentials = new Credentials(token);

            lock (_lockClient)
            {
                _client = github;
            }
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
