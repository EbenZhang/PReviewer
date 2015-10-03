using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Octokit;
using PReviewer.Domain;

namespace PReviewer.Test
{
    [TestFixture]
    public class TestCommitsCombiner
    {
        [Test]
        public void CommitsCombinerShouldInsertNewCommitsBetweenBaseCommitAndHeadCommit()
        {
            var commits = new ObservableCollection<CommitVm>()
            {
                new CommitVm("BASE", "BASESHA"),
                new CommitVm("HEAD", "HEADSHA"),
            };
            var combiner = new CommitsCombiner(commits);

            combiner.Add(new List<PullRequestCommit>
            {
                new PullRequestCommit(null,
                    null, new Commit("URL", "Lable", "Ref", "SHA", null, null, "FirstCommit", null, null, null, new List<GitReference>(), 0),null, null, new List<GitReference>(), "FirstCommit SHA", null),
                new PullRequestCommit(null,
                    null, new Commit("URL", "Lable", "Ref", "SHA", null, null, "SecondCommit", null, null, null, new List<GitReference>(), 0),null, null, new List<GitReference>(), "SecondCommit SHA", null),
            });

            Assert.That(commits.Count, Is.EqualTo(4));
            Assert.That(commits[0].WholeMessage, Is.EqualTo("BASE"));
            Assert.That(commits[1].WholeMessage, Is.EqualTo("FirstCommit"));
            Assert.That(commits[2].WholeMessage, Is.EqualTo("SecondCommit"));
            Assert.That(commits[3].WholeMessage, Is.EqualTo("HEAD"));

            Assert.That(commits[0].Sha, Is.EqualTo("BASESHA"));
            Assert.That(commits[1].Sha, Is.EqualTo("FirstCommit SHA"));
            Assert.That(commits[2].Sha, Is.EqualTo("SecondCommit SHA"));
            Assert.That(commits[3].Sha, Is.EqualTo("HEADSHA"));

            Assert.IsNotNullOrEmpty(commits[0].Display);
            Assert.IsNotNullOrEmpty(commits[1].Display);
            Assert.IsNotNullOrEmpty(commits[2].Display);
            Assert.IsNotNullOrEmpty(commits[3].Display);
        }
    }
}
