using Octokit;
using System.Linq;
using System.Threading.Tasks;

namespace PReviewer.Core.VcsAbstraction.GitHub
{
	public class FileContentRetriever : IFileContentRetriever
	{
		private readonly IGitHubClient _client;

		public FileContentRetriever(IGitHubClient client)
		{
			this._client = client;
		}
		public async Task<string> GetContent(PullRequestLocator pr, string filePath)
		{
			var contents = await _client.Repository.Content.GetAllContents(pr.Owner, pr.Repository, filePath);
			return contents.First().Content;
		}
	}
}
