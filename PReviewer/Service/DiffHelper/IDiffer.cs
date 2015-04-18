using System.Collections.Generic;
using DiffMatchPatch;

namespace PReviewer.Service.DiffHelper
{
    public interface IDiffer
    {
        List<Diff> GetDiffs(string orgText, string newText);
    }

    public class GoogleDifferAdp : IDiffer
    {
        private static readonly diff_match_patch Differ = new diff_match_patch();
        public List<Diff> GetDiffs(string orgText, string newText)
        {
            //return Differ.diff_lineMode(orgText, newText, checklines: true);
            return Differ.diff_main(orgText, newText, checklines: true);
        }
    }
}
