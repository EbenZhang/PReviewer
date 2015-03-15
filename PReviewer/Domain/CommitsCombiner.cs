using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octokit;

namespace PReviewer.Domain
{
    public class CommitsCombiner
    {
        private readonly ObservableCollection<CommitVm> _saveTo;

        public CommitsCombiner(ObservableCollection<CommitVm> saveTo)
        {
            if (saveTo.Count != 2)
            {
                throw new ArgumentException("must have BASE and HEAD commits initially");
            }
            _saveTo = saveTo;
        }

        public void Add(IEnumerable<PullRequestCommit> commits)
        {
            foreach (var c in commits)
            {
                _saveTo.Insert(_saveTo.Count - 1, new CommitVm(c.Commit.Message, c.Sha));
            }
        }
    }
}