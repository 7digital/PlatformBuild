using System;
using System.IO;
using PlatformBuild.LogOutput;

namespace PlatformBuild.CmdLineProxies
{
	public class Git : IGit
	{
		readonly string _git;
		const string FatalHangup = "fatal: The remote end hung up unexpectedly";

		public Git()
		{
			var candidates = new[] {
                @"C:\Program Files (x86)\Git\cmd\git.exe",
                @"C:\Program Files\Git\cmd\git.exe"
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
			repoDir.Call(_git, "pull --ff-only --verbose origin master");
		}

		public void Clone(FilePath repoDir, FilePath filePath, string repo)
		{
			repoDir.Call(_git, "clone " + repo + " " + filePath.Unroot(repoDir).ToPosixPath());
		}

		public void Reset(FilePath path)
		{
			path.Call(_git, "reset --hard HEAD");
		}

		public void PullCurrentBranch(FilePath modulePath, int times = 0)
		{
            if (times > 3)
            {
	            Log.Status("Git server keeps hanging up. Will continue with local copy");
            }
			string s_err="", s_out="";

			if (modulePath.Call(_git, "pull --ff-only --verbose origin", (o,e) => { s_err = e; s_out = o; }) != 0)
			{
				if (s_err.Contains(FatalHangup) || s_out.Contains(FatalHangup))
                {
                    PullCurrentBranch(modulePath, times+1);
                }
				throw new Exception("Git pull failed on " + modulePath.ToEnvironmentalPath() + "; Please resolve and try again");
			}
		}
	}
}