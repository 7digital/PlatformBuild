using System.Diagnostics;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public static class Cmd
	{
		const int twenty_seconds = 20000;
		const int two_minutes = 120000;

		public static int CallInFolder(this FilePath root, string exe, string args)
        {
            return Call(root, root.Navigate((FilePath)exe).ToEnvironmentalPath(), args);
        }

		public static int Call(this FilePath root, string exe, string args)
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = exe,
				Arguments = args,
				ErrorDialog = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				WorkingDirectory = root.ToEnvironmentalPath(),
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};

			var callDescription = root.ToEnvironmentalPath() + ":" + exe + " " + args;
			LogOutput.Log.Verbose(callDescription);

			var proc = Process.Start(processStartInfo);

			if (!proc.WaitForExit(twenty_seconds))
			{
				LogOutput.Log.Error("Call taking a long time: " + callDescription);
				proc.WaitForExit(two_minutes);
				LogOutput.Log.Error("ABORTING LONG CALL " + callDescription);
				// ReSharper disable EmptyGeneralCatchClause
				try { proc.Kill(); }
				catch { }
				// ReSharper restore EmptyGeneralCatchClause
			}

			var messages = proc.StandardOutput.ReadToEnd();
			if (proc.ExitCode != 0) LogOutput.Log.Error(messages);
			else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(messages);

			var errors = proc.StandardError.ReadToEnd();
			if (proc.ExitCode != 0) LogOutput.Log.Error(errors);
			else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(errors);


			return proc.ExitCode;
		}
	}
}