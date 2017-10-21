using System.Threading.Tasks;

namespace PReviewer.Core.VcsAbstraction
{
	public interface IFileContentRetriever
	{
		Task<string> GetContent(PullRequestLocator pr, string filePath);
	}
}
