using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class RecentRepo : ViewModelBase
    {
        public RecentRepo()
        {
            Owners = new ObservableCollection<string>();
            Repositories = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Owners { get; set; }
        public ObservableCollection<string> Repositories { get; set; }

        public async Task Save(PullRequestLocator prInfo, IRepoHistoryPersist persist)
        {
            var hasChanges = false;
            if (!string.IsNullOrWhiteSpace(prInfo.Owner)
                && !Owners.Contains(prInfo.Owner))
            {
                hasChanges = true;
                Owners.Add(prInfo.Owner);
            }
            if (!string.IsNullOrWhiteSpace(prInfo.Repository)
                && !Repositories.Contains(prInfo.Repository))
            {
                hasChanges = true;
                Repositories.Add(prInfo.Repository);
            }
            if (hasChanges)
            {
                await persist.Save(Owners, Repositories);
            }
        }
    }
}