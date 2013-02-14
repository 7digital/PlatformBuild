using System;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public class Git : IGit
	{
		const int _retries = 2;
		readonly string _git;

		public Git()
		{
			var candidates = new[] {
                @"C:\Program Files (x86)\Git\cmd\git.cmd",
                @"C:\Program Files\Git\cmd\git.cmd"
			};

			foreach (var candidate in candidates)
			{
				if (!File.Exists(candidate)) continue;
				_git = candidate;
			}

			if (string.IsNullOrWhiteSpace(_git)) throw new Exception("Couldn't find Git command.");
		}


		public void PullMaster(FilePath repoDir)
		{
			//repoDir.Call("git", "stash");
			repoDir.Call("git", "pull origin master --verbose");
		}

		public void Clone(FilePath repoDir, FilePath filePath, string repo)
		{
			repoDir.Call("git", "clone " + repo + " " + filePath.Unroot(repoDir).ToPosixPath());
		}

		public void Reset(FilePath path)
		{
			path.Call("git", "reset --hard HEAD");
		}

		public void PullCurrentBranch(FilePath modulePath)
		{
			//modulePath.Call("git", "stash");
			for (int i = 0; i < _retries; i++) if (modulePath.Call("git", "pull origin --verbose") == 0) break;

			//if (modulePath.Call("git", "stash pop") != 0)
			//throw new Exception("Possible merge conflicts in " + modulePath.ToEnvironmentalPath());
		}
	}
}