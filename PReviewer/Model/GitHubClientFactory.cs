using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public async Task<IGitHubClient> GetClient(string userName, string password)
        {
            var github = new GitHubClient(new ProductHeaderValue("PReviewer"));

            var credential = new Credentials(userName, password);

            var newAuthorization = new NewAuthorization
            {
                Scopes = new List<string> { "user", "repo" },
                Note = "PReviewer"
            };

            github.Credentials = credential;

            var authorization = await github.Authorization.GetOrCreateApplicationAuthentication(
                 "bf1e8f76de041537f93e",
                "bfe6a01cebe6b96455dedc5572aefcfe7033c9bb",
                newAuthorization);

            github.Connection.Credentials = new Credentials(authorization.Token);

            return github;
        }
    }
}
