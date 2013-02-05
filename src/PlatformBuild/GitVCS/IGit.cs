using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace PlatformBuild.GitVCS
{
	public interface IGit
	{
		FilePath ParentRepo(FilePath currentDirectory);
		void PullCurrent(FilePath repoDir);
	}

	public class Git : IGit
	{
		public FilePath ParentRepo(FilePath currentDirectory)
		{
			return new FilePath(Repository.Discover(currentDirectory.ToEnvironmentalPath()));
		}

		public void PullCurrent(FilePath repoDir)
		{
			using (var repo = new Repository(repoDir.ToEnvironmentalPath()))
			{
				repo.Fetch("origin");
				
			}
		}
	}
}
