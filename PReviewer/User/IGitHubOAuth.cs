using System;
using PReviewer.UI;

namespace PReviewer.User
{
    [Serializable]
    public class GitHubUserCookie
    {
        public string UserName { get; set; }
        public string Token { get; set; }
        public bool IsActiveUser { get; set; }
    }

    public interface IGitHubOAuth
    {
        GitHubUserCookie Login();
    }

    public class GitHubOAuth : IGitHubOAuth
    {
        private readonly Func<GitHubOAuthView> _view;

        public GitHubOAuth(Func<GitHubOAuthView> view)
        {
            _view = view;
        }

        public GitHubUserCookie Login()
        {
            GitHubOAuthView v = _view();
            v.ShowDialog();
            if(!string.IsNullOrWhiteSpace(v.Token))
            {
                return new GitHubUserCookie()
                {
                    UserName = v.UserName,
                    Token = v.Token,
                };
            }
            return null;
        }
    }
}