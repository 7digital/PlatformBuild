using System;
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

		public static int Call(this FilePath root, string exe, string args, Action<string, string> stdOutErr = null)
		{
			var errFile = Path.GetTempFileName();
			var outFile = Path.GetTempFileName();
			var fullArgs = "/c \"" + exe + " " + args + " > " + outFile.Replace(" ", "^ ") + " 2> " + errFile.Replace(" ", "^ ") + "\"";
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "cmd",
				Arguments = fullArgs,
				ErrorDialog = false,
				UseShellExecute = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				WorkingDirectory = root.ToEnvironmentalPath(),
			};

			var callDescription = root.ToEnvironmentalPath() + ":\r\n" + exe + " " + fullArgs;


			var exitCode = RunProcess(stdOutErr, processStartInfo, callDescription);


			var messages = File.ReadAllText(outFile).Trim();
			var errors = File.ReadAllText(errFile).Trim();
			File.Delete(outFile);
			File.Delete(errFile);

			if (exitCode != 0) LogOutput.Log.Error(messages);
			else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(messages);

			if (exitCode != 0) LogOutput.Log.Error(errors);
			else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(errors);

			if (stdOutErr != null) stdOutErr(messages, errors);

			return exitCode;
		}

		static int RunProcess(Action<string, string> stdOutErr, ProcessStartInfo processStartInfo, string callDescription)
		{
			LogOutput.Log.Verbose("cmd started: " + callDescription);
			using (var proc = Process.Start(processStartInfo))
			{
				WaitForFinish(callDescription, proc);

				LogOutput.Log.Verbose("cmd finished: " + callDescription);

				return proc.ExitCode;
			}
		}

		static void WaitForFinish(string callDescription, Process proc)
		{
			if (proc.WaitForExit((int)TimeSpan.FromSeconds(20).TotalMilliseconds)) return;

			proc.Refresh();
			LogOutput.Log.Warning("Call taking a long time, will abort in 2 minutes: " + callDescription);
			if (!proc.WaitForExit((int)TimeSpan.FromMinutes(2).TotalMilliseconds))
			{
				LogOutput.Log.Error("ABORTING LONG CALL: " + callDescription);
				// ReSharper disable EmptyGeneralCatchClause
				try
				{
					proc.Kill();
					proc.Refresh();
				}
				catch
				{
				}
				// ReSharper restore EmptyGeneralCatchClause
			}
		}
	}
}