using Octokit;
using System;

namespace PReviewer.Core.VcsAbstraction.GitHub
{
	public class CommitFile : ICommitFile
	{
		private readonly GitHubCommitFile _commitFile;

		public CommitFile(GitHubCommitFile commitFile)
		{
			_commitFile = commitFile;
			Status = (FileStatus)(Enum.Parse(typeof(FileStatus), _commitFile.Status));
		}	
	
		public FileStatus Status { get; private set; }

		public string Filename => _commitFile.Filename;

		public string Patch => _commitFile.Patch; 

		public string GetFilePath(string sha)
		{
			return _commitFile.GetFilePath(sha);
		}
	}
}
