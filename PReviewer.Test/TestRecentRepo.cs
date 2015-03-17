using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async void ShouldUpdatePRUrlAccordingly()
        {
            var prInfo = new PullRequestLocator { Owner = "1stOwner", Repository = "1stRepo" , PullRequestNumber = 1};
            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            await repoHistory.Save(prInfo, persist);

            Assert.That(repoHistory.PullRequests.Select(r => r.ToUrl()), Contains.Item(prInfo.ToUrl()));

        }

        [Test]
        public void CanCleanHistory_WhenExceedMaxItems()
        {
            var container = new RepoHistoryContainer();
            const int maxItems = 10;
            for (var i = 1; i <= maxItems; ++i)
            {
                container.Urls.Add("https://github.com/ebenzhang/ezplayer/pull/" + i);
            }
            var repoHistory = new RecentRepo { MaxHistoryItems = maxItems };
            repoHistory.From(container);
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(10));
            container.Urls.Add("https://github.com/ebenzhang/ezplayer/pull/21000");
            repoHistory.From(container);
            Assert.That(repoHistory.PullRequests.Count, Is.EqualTo(6), "Half of the MaxItems are removed.");
        }

        [Test]
        public async void WhenRetrieveDiffs_ShouldAdjustRepoPositionInHistory_SoItWillBeTheDefaultRepo()
        {
            var container = new RepoHistoryContainer()
            {
                Owners = new List<string> { "owner1", "owner2" },
                Repositories = new List<string> { "repo1", "repo2" },
                Urls = new List<string> { "https://github.com/owner/repo1/pull/122", "https://github.com/owner/repo2/pull/121" },
            };

            var persist = Substitute.For<IRepoHistoryPersist>();

            var repoHistory = new RecentRepo();
            repoHistory.From(container);
            var prInfo = new PullRequestLocator();
            prInfo.UpdateWith(container.Urls.First());
            await repoHistory.Save(prInfo, persist);

            persist.Received(1).Save(Arg.Is<RepoHistoryContainer>(x => x.Urls.Last() == prInfo.ToUrl())).IgnoreAsyncWarning();
        }
    }
}