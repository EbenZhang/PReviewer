using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PReviewer.User;
using Shouldly;

namespace PReviewer.Test.User
{
    [TestFixture]
    public class TestUserManager
    {
        private IUserProvider _userProvider;
        private IUser _user;
        private UserManager _manager;

        [SetUp]
        public void SetUp()
        {
            _userProvider = Substitute.For<IUserProvider>();
            _user = Substitute.For<IUser>();
            _user.Login().Returns(true);
            _userProvider.CreateUser().Returns(_user);
            _manager = new UserManager();
            _manager.AddNewUser(_userProvider);
        }

        [Test]
        public void CanAddNewUser()
        {
            _userProvider.Received(1).CreateUser();
            _manager.Users.ShouldContain(_user);
        }

        [Test]
        public void ShouldSetCurrenctUserOnceAuthenticated()
        {
            _manager.CurrentUser.ShouldBe(_user);
        }

        [Test]
        public void CanRemoveCurrentUser()
        {
            _manager.RemoveUser(_user);
            _user.Received(1).Delete();
            _manager.Users.ShouldNotContain(_user);
        }

        [Test]
        public void ShouldResetCurrentUserIfRemoved()
        {
            _manager.RemoveUser(_user);
            _manager.CurrentUser = null;
        }

        [Test]
        public void LoginWhenSwitchUser()
        {
            var anotherUser = Substitute.For<IUser>();
            anotherUser.Login().Returns(true);
            _userProvider.CreateUser().Returns(anotherUser);
            _manager.AddNewUser(_userProvider);
            _user.ClearReceivedCalls();
            _manager.CurrentUser = _user;
            _user.Received(1).Login();
        }

        [Test]
        public void CanLogoutUser()
        {
            _manager.Logout();
            _manager.CurrentUser.ShouldBe(null);
        }

        [Test]
        public void CanReplaceExistingUserIfKeyIsTheSame()
        {
            const string key = "key";
            _user.Key.Returns(key);
            var anotherUserWithSameKey = Substitute.For<IUser>();
            anotherUserWithSameKey.Login().Returns(true);
            anotherUserWithSameKey.Key.Returns(key);
            _userProvider.CreateUser().Returns(anotherUserWithSameKey);
            _manager.AddNewUser(_userProvider);
            _manager.Users.Count.ShouldBe(1);
            _manager.Users.ShouldContain(anotherUserWithSameKey);
            _manager.CurrentUser.ShouldBe(anotherUserWithSameKey);
        }

        [Test]
        public void CanLoadUsers()
        {
            var userProvider = Substitute.For<IUserProvider>();
            var user1 = Substitute.For<IUser>();
            var user2 = Substitute.For<IUser>();
            userProvider.Load().Returns(new List<IUser> {user1, user2});
            _manager.LoadUsers(userProvider);
            _manager.Users.ShouldContain(user1);
            _manager.Users.ShouldContain(user2);
        }

        [Test]
        public void CanLoadLoggedInUser()
        {
            var userProvider = Substitute.For<IUserProvider>();
            var user1 = Substitute.For<IUser>();
            var user2 = Substitute.For<IUser>();
            user2.IsActiveUser.Returns(true);
            userProvider.Load().Returns(new List<IUser> { user1, user2 });
            _manager.LoadUsers(userProvider);
            _manager.CurrentUser.ShouldBe(user2);
            _manager.Users.Count.ShouldBe(2);
        }

        [Test]
        public void SetOtherAccountAsInactivate()
        {
            var anotherUser = Substitute.For<IUser>();
            _userProvider.CreateUser().Returns(anotherUser);
            anotherUser.Login().Returns(true);
            _manager.AddNewUser(_userProvider);
            anotherUser.IsActiveUser.ShouldBe(true);
            _manager.CurrentUser = _user;
            anotherUser.IsActiveUser.ShouldBe(false);
        }
    }
}

