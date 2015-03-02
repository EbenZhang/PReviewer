using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ExtendedCL;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class RepoHistoryPersist : IRepoHistoryPersist
    {
        private static readonly string RepoHistoryFile = Path.Combine(PathHelper.ProcessAppDir, "repohistory.xml");

        public async Task Save(IEnumerable<string> owners, IEnumerable<string> repositories)
        {
            if (File.Exists(RepoHistoryFile))
            {
                File.Delete(RepoHistoryFile);
            }

            var container = new RepoHistoryContainer();
            container.Owners.AddRange(owners);
            container.Repositories.AddRange(repositories);

            await Task.Run(() =>
            {
                using (var stream = File.OpenWrite(RepoHistoryFile))
                {
                    new XmlSerializer(typeof (RepoHistoryContainer)).Serialize(stream, container);
                }
            });
        }

        public async Task<RepoHistoryContainer> Load()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(RepoHistoryFile))
                {
                    return new RepoHistoryContainer();
                }
                using (var stream = File.OpenRead(RepoHistoryFile))
                {
                    return new XmlSerializer(typeof (RepoHistoryContainer)).Deserialize(stream) as RepoHistoryContainer;
                }
            });
        }
    }
}