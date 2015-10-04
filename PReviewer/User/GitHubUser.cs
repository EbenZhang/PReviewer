using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExtendedCL;
using GalaSoft.MvvmLight;

namespace PReviewer.User
{
    public class GitHubUser : ViewModelBase, IUser
    {
        public IGitHubOAuth OAuth;
        public IGitHubUserPersister Persister;

        public static readonly string GithubUserPrefix = "GitHub - ";

        public GitHubUser(IGitHubOAuth oAuth, IGitHubUserPersister persister)
        {
            OAuth = oAuth;
            Persister = persister;
        }

        public string Key
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(UserCookie.UserName))
                {
                    return GithubUserPrefix + UserCookie.UserName;
                }
                return "";
            }
        }

        public GitHubUserCookie UserCookie { get; set; }

        public bool Login()
        {
            if (string.IsNullOrWhiteSpace(UserCookie?.Token))
            {
                UserCookie = OAuth.Login();
            }
            if (UserCookie == null)
            {
                return false;
            }
            Persister.Save(this);
            return true;
        }

        public void Logout()
        {
            IsActiveUser = false;
        }

        public void Delete()
        {
            Persister.Delete(this);
        }

        public bool IsActiveUser
        {
            get { return UserCookie.IsActiveUser; }
            set
            {
                UserCookie.IsActiveUser = value;
                Persister.Save(this);
                RaisePropertyChanged();
            }
        }

        public string Token => UserCookie.Token;
    }

    public interface IGitHubUserPersister
    {
        void Save(GitHubUser user);
        void Delete(GitHubUser user);
        IEnumerable<IUser> Load();
    }

    public class GitHubUserPersister : IGitHubUserPersister
    {
        private readonly IGitHubOAuth _oAuth;

        public GitHubUserPersister(IGitHubOAuth oAuth)
        {
            _oAuth = oAuth;
        }

        private static readonly string GitHubUsersDir = Path.Combine(PathHelper.ProcessAppDir, "Users", "GitHub");
        public void Save(GitHubUser user)
        {
            EnsureDirectoryExists();
            var filePath = Path.Combine(GitHubUsersDir, user.UserCookie.UserName + ".bin");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, user.UserCookie);
            }
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(GitHubUsersDir))
            {
                Directory.CreateDirectory(GitHubUsersDir);
            }
        }

        public void Delete(GitHubUser user)
        {
            var filePath = Path.Combine(GitHubUsersDir, user.UserCookie.UserName);

            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // ignored
            }
        }

        public IEnumerable<IUser> Load()
        {
            EnsureDirectoryExists();
            foreach (var file in Directory.EnumerateFiles(GitHubUsersDir, "*.bin", SearchOption.TopDirectoryOnly))
            {
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    var bFormatter = new BinaryFormatter();

                    var user = new GitHubUser(_oAuth, this);
                    try
                    {
                        var cookie = bFormatter.Deserialize(stream) as GitHubUserCookie;
                        user.UserCookie = cookie;
                    }
                    catch
                    {
                        yield break;
                    }
                    yield return user;
                }
            }
        }
    }
}