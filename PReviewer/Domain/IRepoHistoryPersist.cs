using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PReviewer.Domain
{
    [Serializable]
    public class RepoHistoryContainer : IEquatable<RepoHistoryContainer>
    {
        public RepoHistoryContainer()
        {
            Owners = new List<string>();
            Repositories = new List<string>();
            Urls = new List<string>();
        }

        public List<string> Owners { get; set; }
        public List<string> Repositories { get; set; }
        public List<string> Urls{ get; set; }

        public bool Equals(RepoHistoryContainer obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Owners.SequenceEqual(obj.Owners)
            && Repositories.SequenceEqual(obj.Repositories)
            && Urls.SequenceEqual(obj.Urls);
        }
    }

    public interface IRepoHistoryPersist
    {
        Task<RepoHistoryContainer> Load();
        Task Save(RepoHistoryContainer container);
    }
}