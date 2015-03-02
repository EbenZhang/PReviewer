using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace PReviewer.Domain
{
    public interface ICommentsBuilder
    {
        string Build(IEnumerable<CommitFileVm> filesWithComments, 
            string generalComments);
    }
}
