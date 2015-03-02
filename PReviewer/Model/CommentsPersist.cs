using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class CommentsPersist : ICommentsPersist
    {
        public async Task Save(PullRequestLocator prInfo, IEnumerable<CommitFileVm> diffs, string generalComments)
        {
            var commentsFile = GetCommentsFilePath(prInfo);
            await Task.Run(() =>
            {
                using (var stream = File.OpenWrite(commentsFile))
                {
                    new XmlSerializer(typeof (CommentsContainer))
                        .Serialize(stream,
                            CommentsContainer.From(diffs, generalComments));
                }
            });
        }

        private static string GetCommentsFilePath(PullRequestLocator prInfo)
        {
            var dir = FileContentPersist.GetPullRequestDir(prInfo);
            var commentsFile = Path.Combine(dir, "comments.xml");
            return commentsFile;
        }

        public async Task<CommentsContainer> Load(PullRequestLocator prInfo)
        {
            var commentsFile = GetCommentsFilePath(prInfo);
            return await Task.Run(() =>
            {
                if (!File.Exists(commentsFile))
                {
                    return new CommentsContainer();
                }
                using (var stream = File.OpenRead(commentsFile))
                {
                    var container = new XmlSerializer(typeof (CommentsContainer)).Deserialize(stream) as CommentsContainer;
                    return container;
                }
            });
        }
    }
}
