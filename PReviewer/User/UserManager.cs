using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using GalaSoft.MvvmLight;
using WpfCommon.Utils;

namespace PReviewer.User
{
    public interface IUserManager
    {
        bool AddNewUser(IUserProvider userProvider);
        ObservableCollection<IUser> Users { get; set; }
        IUser CurrentUser { get; set; }
        void RemoveUser(IUser user);
        void Logout();
        void LoadUsers(IUserProvider userProvider);
    }

    public class UserManager : ViewModelBase, IUserManager
    {
        private IUser _currentUser;

        public bool AddNewUser(IUserProvider userProvider)
        {
            var user = userProvider.CreateUser();
            if (user.Login())
            {
                var existing = Users.FirstOrDefault(r => r.Key == user.Key);
                if (existing != null)
                {
                    Users.Remove(existing);
                }
                Users.Add(user);
                CurrentUser = user;
                return true;
            }
            return false;
        }

        public ObservableCollection<IUser> Users { get; set; } = new ObservableCollection<IUser>();

        public IUser CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;

                if (_currentUser != null)
                {
                    foreach (var user in Users)
                    {
                        user.IsActiveUser = false;
                    }

                    _currentUser.Login();

                    _currentUser.IsActiveUser = true;
                }

                RaisePropertyChanged();
            }
        }

        public void RemoveUser(IUser user)
        {
            user.Delete();
            if (CurrentUser == user)
            {
                CurrentUser = null;
            }

            Users.Remove(user);
        }

        public void Logout()
        {
            CurrentUser.Logout();
            CurrentUser = null;
        }

        public void LoadUsers(IUserProvider userProvider)
        {
            Users.Clear();

            Users.AddRange(userProvider.Load());
            _currentUser = Users.FirstOrDefault(r => r.IsActiveUser);
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(CurrentUser));
        }
    }
}