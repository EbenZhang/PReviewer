using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class CommentsBuilder : ICommentsBuilder
    {
        public string Build(IEnumerable<CommitFileVm> filesWithComments, 
            string generalComments)
        {
            var ret = (from file in filesWithComments
                where !string.IsNullOrWhiteSpace(file.Comments)
                       select FormatComments(file.GitHubCommitFile.Filename, file.Comments)).ToList();

            if (!string.IsNullOrWhiteSpace(generalComments))
            {
                ret.Add(FormatComments("General Comments", generalComments));
            }
            return string.Join("\r\n", ret.ToArray());
        }

        private static string FormatComments(string header, string comments)
        {
            return string.Format(@"###### *{0}*

{1}", header, comments);
        }
    }
}
