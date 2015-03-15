using System;
using Octokit;

namespace PReviewer.Test
{
    internal class MockRepositoryContent : RepositoryContent
    {
        public new string EncodedContent
        {
            set { base.EncodedContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes((value))); }
        }
    }
}