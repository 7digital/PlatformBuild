using System.Diagnostics;
using System.IO;

namespace PlatformBuild.GitVCS
{
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
}