using System.Collections.Generic;
using Octokit;

namespace PReviewer.Test
{
    internal class MockCompareResult : CompareResult
    {
        public MockGitHubCommitFile File1 = new MockGitHubCommitFile
        {
            Sha = "e74fe8d371a5e33c4877f662e6f8ed7c0949a8b0",
            Filename = "test.xaml",
            Patch = "Patch"
        };

        public MockGitHubCommitFile File2 = new MockGitHubCommitFile
        {
            Sha = "9dc7f01526e368a64c49714c51f1d851885793ba",
            Filename = "app.xaml.cs"
        };

        public MockCompareResult()
        {
            Files = new List<GitHubCommitFile>
            {
                File1,
                File2
            };
            var mockBasCommit = new MockGitHubCommit {Sha = "ef4f2857776d06cc28acedbd023bbb33ca83d216"};
            BaseCommit = mockBasCommit;
        }
    }
}