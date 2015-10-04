using System;
using System.Diagnostics;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PReviewer.User;

namespace PReviewer.Domain
{
    public class LoginWndVm : ViewModelBase
    {
        public readonly IGitHubClientFactory GitHubClientFactory;
        public IUserManager UserManager { get; }
        private readonly GitHubUserProvider _userProvider;

        public LoginWndVm(IGitHubClientFactory gitHubClientFactory,
            IUserManager userManager, GitHubUserProvider userProvider)
        {
            GitHubClientFactory = gitHubClientFactory;
            UserManager = userManager;
            _userProvider = userProvider;
        }

        public void LoadCredential()
        {
            UserManager.LoadUsers(_userProvider);
        }

        public void Login()
        {
            UserManager.CurrentUser?.Login();
        }

        public bool HasActiveUser => UserManager?.CurrentUser != null;

        public ICommand AddNewUserCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UserManager.CurrentUser.IsActiveUser = false;
                    // restart the program, otherwise the webbrowser remembers the cookie even if deleted.
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Environment.Exit(0);
                });
            }
        }

        public ICommand ConfirmSelection
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool AddNewUser()
        {
            return UserManager.AddNewUser(_userProvider);
        }

        public void CreateGitHubClientWhenTokenAlreadyGot()
        {
            GitHubClientFactory.CreateClient(UserManager.CurrentUser.Token);
        }
    }
}
