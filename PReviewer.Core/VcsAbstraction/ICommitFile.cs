namespace PReviewer.Core.VcsAbstraction
{
	public enum FileStatus
	{
		Removed,
		Renamed,
		New,
		Modified,
		Changed,
	}

	public interface ICommitFile
	{
		FileStatus Status { get; }
		string Filename { get; }
		string Patch { get; }

		string GetFilePath(string sha);
	}
}
