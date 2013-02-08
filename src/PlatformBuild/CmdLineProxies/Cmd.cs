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
			var isBatch = false;//exe.EndsWith(".cmd") || exe.EndsWith(".bat");

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
				RedirectStandardOutput = true,
			};

			var callDescription = root.ToEnvironmentalPath() + ":" + bin + " " + rargs;
			LogOutput.Log.Verbose(callDescription);

			var proc = Process.Start(processStartInfo);

			if (!proc.WaitForExit(10000))
			{
				LogOutput.Log.Error("Call taking a long time: " + callDescription);
				proc.WaitForExit(10000);
				LogOutput.Log.Error("ABORTING LONG CALL " + callDescription);
			}

			var errors = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(errors)) LogOutput.Log.Error(proc.StandardError.ReadToEnd());

            var messages = proc.StandardOutput.ReadToEnd();
			if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(messages);

			return proc.ExitCode;
		}
	}
}