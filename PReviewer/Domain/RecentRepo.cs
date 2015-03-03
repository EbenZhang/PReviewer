using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using ExtendedCL;
using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class RecentRepo : ViewModelBase
    {
        public RecentRepo()
        {
            Owners = new ObservableCollection<string>();
            Repositories = new ObservableCollection<string>();
            PullRequests = new ObservableCollection<PullRequestLocator>();
        }

        public ObservableCollection<string> Owners { get; set; }
        public ObservableCollection<string> Repositories { get; set; }
        public ObservableCollection<PullRequestLocator> PullRequests { get; private set; }

        public async Task Save(PullRequestLocator prInfo, IRepoHistoryPersist persist)
        {
            if (!prInfo.IsValid())
            {
                return;
            }

            if (PullRequests.Contains(prInfo))
            {
                return;
            }

            if (!Owners.Contains(prInfo.Owner))
            {
                Owners.Add(prInfo.Owner);
            }
            if (!Repositories.Contains(prInfo.Repository))
            {
                Repositories.Add(prInfo.Repository);
            }

            PullRequests.Add(new PullRequestLocator()
            {
                Owner = prInfo.Owner,
                Repository =  prInfo.Repository,
                PullRequestNumber =  prInfo.PullRequestNumber,
            });
            await persist.Save(ToContainer());
        }

        public void From(RepoHistoryContainer historyContainer)
        {
            Owners.Assign(historyContainer.Owners);
            Repositories.Assign(historyContainer.Repositories);
            PullRequests.Assign(historyContainer.Urls.Select(PullRequestLocator.FromUrl));
        }

        public RepoHistoryContainer ToContainer()
        {
            var container = new RepoHistoryContainer();
            container.Owners.AddRange(Owners);
            container.Repositories.AddRange(Repositories);
            container.Urls.AddRange(PullRequests.Select(p => p.ToUrl()));
            return container;
        }
    }
}