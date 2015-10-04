using NSubstitute;
using NUnit.Framework;
using PReviewer.User;
using Shouldly;

namespace PReviewer.Test.User
{
    [TestFixture]
    public class TestGitHubUser
    {
        private IGitHubOAuth _oAuth;
        private GitHubUser _user;
        private string _userName;
        private string _token;
        private IGitHubUserPersister _persister;

        [SetUp]
        public void SetUp()
        {
            _oAuth = Substitute.For<IGitHubOAuth>();
            _persister = Substitute.For<IGitHubUserPersister>();
            _userName = "UserName";
            _token = "token";
            _oAuth.Login().Returns(new GitHubUserCookie()
            {
                UserName = _userName,
                Token = _token
            });
            _user = new GitHubUser(_oAuth, _persister);
        }

        [Test]
        public void CanLogin()
        {   
            _user.Login();
            _oAuth.Received(1).Login();
            _user.Key.ShouldBe(GitHubUser.GithubUserPrefix + _userName);
            _user.UserCookie.UserName.ShouldBe(_userName);
            _user.UserCookie.Token.ShouldBe(_token);
            _persister.Received(1).Save(_user);
        }

        [Test]
        public void CanLogout()
        {
            _user.Login();
            _user.Logout();
            _user.IsActiveUser.ShouldBe(false);
        }
    }
}
