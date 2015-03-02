using NSubstitute;
using NUnit.Framework;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestRecentRepo
    {
        [Test]
        public async void ShouldSaveEitherOwnersOrRepositoriesChanged()
        {
            var prInfo = new PullRequestLocator {Owner = "1stOwner", Repository = "1stRepo"};

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.Owners.Count, Is.EqualTo(1));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(1));
            persist.Received(1).Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Owner += "changed owner";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(2));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(1));
            persist.Received(1).Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Repository += "changed repo";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(2));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(2));
            persist.Received(1).Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();
        }

        [Test]
        public async void ShouldExcludeEmptyOwners()
        {
            var prInfo = new PullRequestLocator {Owner = "", Repository = ""};

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.Owners.Count, Is.EqualTo(0));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.DidNotReceiveWithAnyArgs().Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();

            prInfo.Owner += "changed owner";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(1));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.Received(1).Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Owner = null;
            prInfo.Repository += "changed repo";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(1));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(1));
            persist.Received(1).Save(repoHistory.Owners, repoHistory.Repositories).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();
        }

        [Test]
        public async void ShouldntAddDuplicateRepoToHistory()
        {
            var prInfo = new PullRequestLocator { Owner = "1stOwner", Repository = "1stRepo" };

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);
            await repoHistory.Save(prInfo, persist);
            persist.ReceivedWithAnyArgs(1).Save(null, null).IgnoreAsyncWarning();
        }
    }
}