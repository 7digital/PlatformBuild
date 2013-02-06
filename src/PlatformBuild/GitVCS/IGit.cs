using System.Diagnostics;
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

	public static class Cmd
	{
		public static int Call(this FilePath root, string exe, string args)
		{
			var proc = Process.Start(new ProcessStartInfo {
				FileName = exe,
				Arguments = args,
				ErrorDialog = false,
				UseShellExecute = true,
				CreateNoWindow = true,
				WorkingDirectory = root.ToEnvironmentalPath()
			});
			proc.WaitForExit();
			return proc.ExitCode;
		}
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
			throw new System.NotImplementedException();
		}

		public void PullCurrentBranch(FilePath modulePath)
		{
			throw new System.NotImplementedException();
		}
	}
}
