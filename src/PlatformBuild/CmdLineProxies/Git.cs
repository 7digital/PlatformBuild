using System;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public class Git : IGit
	{
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
			repoDir.Call("git", "pull origin master --ff-only --verbose");
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
			if (modulePath.Call("git", "pull origin --ff-only --verbose") != 0)
				throw new Exception("Git pull failed on " + modulePath.ToEnvironmentalPath() + "; Please resolve and try again");
		}
	}
}