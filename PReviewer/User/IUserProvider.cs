using System.Collections.Generic;

namespace PReviewer.User
{
    public interface IUserProvider
    {
        IUser CreateUser();
        IEnumerable<IUser> Load();
    }

    public class GitHubUserProvider : IUserProvider
    {
        private readonly IGitHubOAuth _oAuth;
        private readonly IGitHubUserPersister _persister;

        public GitHubUserProvider(IGitHubOAuth oAuth, IGitHubUserPersister persister)
        {
            _oAuth = oAuth;
            _persister = persister;
        }

        public IUser CreateUser()
        {
            return new GitHubUser(_oAuth, _persister);
        }

        public IEnumerable<IUser> Load()
        {
            return _persister.Load();
        }
    }
}