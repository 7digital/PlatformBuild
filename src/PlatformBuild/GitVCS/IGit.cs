using System.IO;
using System.Linq;

namespace PlatformBuild.GitVCS
{
	public interface IGit
	{
		void PullMaster(FilePath repoDir);
		void Clone(FilePath repoDir, FilePath filePath, string repo);
		void ResetLib(FilePath modulePath);
		void PullCurrentBranch(FilePath modulePath);
	}

	public class Git : IGit
	{
		public void PullMaster(FilePath repoDir)
		{
			repoDir.Call("git", "stash");
			repoDir.Call("git", "pull origin master");
		}

		public void Clone(FilePath repoDir, FilePath filePath, string repo)
		{
			repoDir.Call("git", "clone "+repo+" "+filePath.Unroot(repoDir).ToPosixPath());
		}

		public void ResetLib(FilePath modulePath)
		{
			var path = modulePath.Navigate(new FilePath("lib"));
            path.Call("git", "reset --hard HEAD");
		}

		public void PullCurrentBranch(FilePath modulePath)
		{
			throw new System.NotImplementedException();
		}
	}
}
