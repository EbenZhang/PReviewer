using System.Collections;
using System.Collections.Generic;
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
        private const int MAX_HISTORY_ITEMS = 100;
        private int _maxHistoryItems = MAX_HISTORY_ITEMS;

        public int MaxHistoryItems
        {
            get { return _maxHistoryItems; }
            set { _maxHistoryItems = value; }
        }

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

            var newPrInfo = new PullRequestLocator()
            {
                Owner = prInfo.Owner,
                Repository = prInfo.Repository,
                PullRequestNumber = prInfo.PullRequestNumber,
            };

            if (PullRequests.Contains(prInfo))
            {
                PullRequests.Remove(prInfo);
            }
            PullRequests.Insert(0, newPrInfo);

            if (!Owners.Contains(prInfo.Owner))
            {
                Owners.Insert(0, prInfo.Owner);
            }
            if (!Repositories.Contains(prInfo.Repository))
            {
                Repositories.Insert(0, prInfo.Repository);
            }
            
            await persist.Save(ToContainer());
        }

        public void From(RepoHistoryContainer historyContainer)
        {
            Owners.Assign(RemoveIfExceedMax(historyContainer.Owners));
            Repositories.Assign(RemoveIfExceedMax(historyContainer.Repositories));
            PullRequests.Assign(RemoveIfExceedMax(historyContainer.Urls.Select(PullRequestLocator.FromUrl).ToList()));
        }

        private IList<T> RemoveIfExceedMax<T>(IList<T> orgList)
        {
            return orgList.Count > MaxHistoryItems ? orgList.Take(MaxHistoryItems/2).ToList() : orgList;
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