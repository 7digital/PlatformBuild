using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public class Git : IGit
	{
		const int _retries = 2;

		public void PullMaster(FilePath repoDir)
		{
			//repoDir.Call("git", "stash");
			repoDir.Call("git", "pull origin master --progress");
		}

		public void Clone(FilePath repoDir, FilePath filePath, string repo)
		{
			repoDir.Call("git", "clone "+repo+" "+filePath.Unroot(repoDir).ToPosixPath());
		}

		public void Reset(FilePath path)
		{
			path.Call("git", "reset --hard HEAD");
		}

		public void PullCurrentBranch(FilePath modulePath)
		{
			//modulePath.Call("git", "stash");
			for (int i = 0; i < _retries; i++) if (modulePath.Call("git", "pull origin --progress") == 0) break;
			
            //if (modulePath.Call("git", "stash pop") != 0)
				//throw new Exception("Possible merge conflicts in " + modulePath.ToEnvironmentalPath());
		}
	}
}