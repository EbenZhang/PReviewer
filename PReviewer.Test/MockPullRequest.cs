using Octokit;

namespace PReviewer.Test
{
    internal class MockPullRequest : PullRequest
    {
        public MockPullRequest()
        {
            Title = "Title";
            Body = @"# 1st head
paragraph1
# 2nd head
paragraph2
";
            Base = new GitReference("", "BASE", "", "1212", null, null);
            Head = new GitReference("", "HEAD", "", "asdfasdf", null, null);
        }

        public new int Number
        {
            get { return base.Number; }
            set { base.Number = value; }
        }

        public new string Body
        {
            get { return base.Body; }
            set { base.Body = value; }
        }
    }
}