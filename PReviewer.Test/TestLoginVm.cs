using System;
using ExtendedCL;
using NSubstitute;
using NUnit.Framework;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestLoginVm
    {
        private IGitHubClientFactory _clientFactory;
        private ICredentialPersisit _credentialPersist;

        [SetUp]
        public void SetUp()
        {
            _clientFactory = Substitute.For<IGitHubClientFactory>();
            _credentialPersist = Substitute.For<ICredentialPersisit>();
        }
        [Test]
        public void GivenAEmptyUserName_NoNeedToSaveCredential()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "" };
            vm.SaveCredential();
            _credentialPersist.DidNotReceive().Save(Arg.Any<LoginCredential>());

            vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = null };
            vm.SaveCredential();
            _credentialPersist.DidNotReceive().Save(Arg.Any<LoginCredential>());
        }
        
        [Test]
        public void CanLoadCredential()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "", Password = ""};
            var expectedCredential = new LoginCredential() {UserName = "asdf", Password = "kkk"};
            _credentialPersist.Load().Returns(expectedCredential);
            vm.LoadCredential();

            _credentialPersist.Received(1).Load();
            Assert.That(vm.UserName, Is.EqualTo(expectedCredential.UserName));
            Assert.That(vm.Password, Is.EqualTo(expectedCredential.Password));
        }

        [Test]
        public void GivenAnEmptyCredential_ShouldntBePopulated()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "", Password = "" };
            var expectedCredential = new LoginCredential() { UserName = "", Password = "" };
            _credentialPersist.Load().Returns(expectedCredential);
            vm.LoadCredential();

            _credentialPersist.Received(1).Load();
            Assert.IsEmpty(vm.UserName);
            Assert.IsEmpty(vm.Password);
        }

        [Test]
        public async void CanLogin()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "11", Password = "22" };
            await vm.Login();

            _clientFactory.Received(1).Login(vm.UserName, vm.Password).IgnoreAsyncWarning();
        }

        [Test]
        public async void ShouldChangeTheIsProcessingStatusWhenLogin()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "11", Password = "22" };
            var changeCount = 0;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((LoginWndVm v) => v.IsProcessing))
                {
                    changeCount++;
                }
            };
            await vm.Login();

            _clientFactory.Received(1).Login(vm.UserName, vm.Password).IgnoreAsyncWarning();

            Assert.That(changeCount, Is.EqualTo(2), "IsProcessing should be changed twice, first it's changed to busy, and then back to false.");
            Assert.False(vm.IsProcessing, "Status should back to non-busy");
        }

        [Test]
        public void GivenAFailureOfLogin_TheIsProcessingStatusShouldBackToFalseAnyway()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "11", Password = "22" };
            var changeCount = 0;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == PropertyName.Get((LoginWndVm v) => v.IsProcessing))
                {
                    changeCount++;
                }
            };

            _clientFactory.When(x => x.Login(Arg.Any<string>(), Arg.Any<string>())).Do(x => { throw new Exception(); });

            Assert.Throws<Exception>(async () =>
            {
                await vm.Login();
            });

            _clientFactory.Received(1).Login(vm.UserName, vm.Password).IgnoreAsyncWarning();

            Assert.That(changeCount, Is.EqualTo(2), "IsProcessing should be changed twice, first it's changed to busy, and then back to false.");
            Assert.False(vm.IsProcessing, "Status should back to non-busy");
        }

        [Test]
        public void GivenAFailedLogin_NoCredentialShouldBeSaved()
        {
            var vm = new LoginWndVm(_clientFactory, _credentialPersist) { UserName = "11", Password = "22" };
            _clientFactory.When(x => x.Login(Arg.Any<string>(), Arg.Any<string>())).Do(x => { throw new Exception(); });
            Assert.Throws<Exception>(async () =>
            {
                await vm.Login();
            });
            _credentialPersist.DidNotReceive().Save(Arg.Any<LoginCredential>());
        }
    }

}
