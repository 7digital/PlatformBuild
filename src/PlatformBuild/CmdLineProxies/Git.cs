using System;
using System.IO;
using System.Linq;
using PlatformBuild.LogOutput;

namespace PlatformBuild.CmdLineProxies
{
	public class Git : IGit
	{
		readonly string _git;
		const string FatalHangup = "fatal: The remote end hung up unexpectedly";

		public Git()
		{
			_git = FindGit();
			if (string.IsNullOrWhiteSpace(_git)) throw new Exception("Couldn't find Git command.");
		}

		static string FindGit()
		{
			var candidates = (Environment.GetEnvironmentVariable("PATH") ?? "")
				.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
				.Where(p => p.ToLowerInvariant().Contains("git"))
				.Select(s => s + "\\").ToList();
			candidates.Add(@"C:\Program Files (x86)\Git\cmd\");
			candidates.Add(@"C:\Program Files\Git\cmd\");

			foreach (var candidate in candidates)
			{
				var a = candidate + "..\\bin\\git.exe";
				var b = candidate + "git.exe";

                if (File.Exists(a)) return a;
                if (File.Exists(b)) return b;
			}
            return null;
		}


		public void PullMaster(FilePath repoDir)
		{
			repoDir.Call(_git, "pull --ff-only --verbose origin master");
		}

		public void Clone(FilePath repoDir, FilePath filePath, string repo)
		{
			repoDir.Call(_git, "clone " + repo + " " + filePath.Unroot(repoDir).ToPosixPath());
		}

		public void CheckoutFolder(FilePath path)
		{
			path.Call(_git, "checkout . --theirs");
		}

		public void PullCurrentBranch(FilePath modulePath, int times = 0)
		{
			if (times > 3)
			{
				Log.Status("Git server keeps hanging up. Will continue with local copy");
			}
			string s_err = "", s_out = "";
            string branch = "";
            modulePath.Call(_git, "status --branch --short", (o,e) => {
                branch = GuessBranch(o+e);
            });

			if (modulePath.Call(_git, "pull --ff-only --verbose origin "+branch, (o, e) => { s_err = e; s_out = o; }) != 0)
			{
				if (s_err.Contains(FatalHangup) || s_out.Contains(FatalHangup))
				{
					PullCurrentBranch(modulePath, times + 1);
				}
				else throw new Exception("Git pull failed on " + modulePath.ToEnvironmentalPath() + "; Please resolve and try again");
			}
		}

		static string GuessBranch(string output)
		{
            const string tag = "## ";
			const string defaultBranch = "master";

			var idx = output.IndexOf(tag, StringComparison.Ordinal);

			if (idx < 0) return defaultBranch;
            idx += tag.Length;

            var idx2 = output.IndexOfAny(new []{'\r', '\n'}, idx);
            if (idx2 < 0) return defaultBranch;

			var guess = output.Substring(idx, idx2-idx);

			return guess.Contains("...") ? "" : guess;
		}
	}
}