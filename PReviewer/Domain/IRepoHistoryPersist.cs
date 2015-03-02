using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PReviewer.Domain
{
    [Serializable]
    public class RepoHistoryContainer
    {
        public RepoHistoryContainer()
        {
            Owners = new List<string>();
            Repositories = new List<string>();
        }

        public List<string> Owners { get; set; }
        public List<string> Repositories { get; set; }
    }

    public interface IRepoHistoryPersist
    {
        Task Save(IEnumerable<string> owners, IEnumerable<string> repositories);
        Task<RepoHistoryContainer> Load();
    }
}