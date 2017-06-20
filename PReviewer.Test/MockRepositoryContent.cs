using System;
using Octokit;

namespace PReviewer.Test
{
    internal class MockRepositoryContent : RepositoryContent
    {
        public MockRepositoryContent(string encodedContent) : base(null, null, null, 0, ContentType.File, null, null,
            null, null, "", encodedContent, null, null)
        {
        }
    }
}