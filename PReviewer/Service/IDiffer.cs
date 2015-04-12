﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffMatchPatch;

namespace PReviewer.Service
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
