using System.Linq;
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
            var prInfo = new PullRequestLocator {Owner = "1stOwner", Repository = "1stRepo", PullRequestNumber = 1};

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.Owners.Count, Is.EqualTo(1));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(1));
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(1));
            persist.Received(1).Save(Arg.Is<RepoHistoryContainer>(r =>
                r.Equals(repoHistory.ToContainer()))).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Owner += "changed owner";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(2));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(1));
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(2));
            persist.Received(1).Save(Arg.Is<RepoHistoryContainer>(r =>
                r.Equals(repoHistory.ToContainer()))).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Repository += "changed repo";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(2));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(2));
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(3));
            persist.Received(1).Save(Arg.Is<RepoHistoryContainer>(r =>
                r.Equals(repoHistory.ToContainer()))).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();


            prInfo.PullRequestNumber++;
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(2));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(2));
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(4));

            persist.Received(1).Save(Arg.Is<RepoHistoryContainer>(r =>
                r.Equals(repoHistory.ToContainer()))).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();
        }

        [Test]
        public async void ShouldExcludeEmptyOwners()
        {
            var prInfo = new PullRequestLocator {Owner = "", Repository = "", PullRequestNumber = 1};

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.Owners.Count, Is.EqualTo(0));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.DidNotReceiveWithAnyArgs().Save(null).IgnoreAsyncWarning();

            prInfo.Owner += "changed owner";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(0));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.DidNotReceiveWithAnyArgs().Save(null).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Owner = null;
            prInfo.Repository += "changed repo";
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(0));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.DidNotReceiveWithAnyArgs().Save(null).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();

            prInfo.Owner = "owner";
            prInfo.Repository = "repo";
            prInfo.PullRequestNumber = 0;
            await repoHistory.Save(prInfo, persist);
            Assert.That(repoHistory.Owners.Count, Is.EqualTo(0));
            Assert.That(repoHistory.Repositories.Count, Is.EqualTo(0));
            persist.DidNotReceiveWithAnyArgs().Save(null).IgnoreAsyncWarning();
            persist.ClearReceivedCalls();
        }

        [Test]
        public async void ShouldntAddDuplicateRepoToHistory()
        {
            var prInfo = new PullRequestLocator { Owner = "1stOwner", Repository = "1stRepo", PullRequestNumber = 1};

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);
            await repoHistory.Save(prInfo, persist);
            persist.ReceivedWithAnyArgs(1).Save(null).IgnoreAsyncWarning();
        }

        [Test]
        public async void ShouldUpdatePRUrlAccordingly()
        {
            var prInfo = new PullRequestLocator { Owner = "1stOwner", Repository = "1stRepo" , PullRequestNumber = 1};
            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.PullRequests.Select(r => r.ToUrl()), Contains.Item(prInfo.ToUrl()));

        }
    }
}