using System.Diagnostics;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public static class Cmd
	{
        public static int CallInFolder(this FilePath root, string exe, string args)
        {
            return Call(root, root.Navigate((FilePath)exe).ToEnvironmentalPath(), args);
        }

		public static int Call(this FilePath root, string exe, string args)
		{
			var isBatch = exe.EndsWith(".cmd") || exe.EndsWith(".bat");

			var bin = (isBatch) ? ("cmd") : (exe);
			var rargs = (isBatch) ? ("/c " + exe + " " + args) : args;

			var processStartInfo = new ProcessStartInfo
			{
				FileName = bin,
				Arguments = rargs,
				ErrorDialog = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				WorkingDirectory = root.ToEnvironmentalPath(),
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			LogOutput.Log.Verbose(root.ToEnvironmentalPath()+":"+bin + " " + rargs);

			var proc = Process.Start(processStartInfo);

			proc.WaitForExit();

            // TODO: read before exit?
			LogOutput.Log.Error(proc.StandardError.ReadToEnd());
			LogOutput.Log.Info(proc.StandardOutput.ReadToEnd());

			return proc.ExitCode;
		}
	}
}