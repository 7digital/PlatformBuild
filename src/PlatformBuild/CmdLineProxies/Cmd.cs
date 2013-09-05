using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RunProcess;

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
			using (var proc = new ProcessHost(exe, root.ToEnvironmentalPath()))
			{
				proc.Start(args);
				int exitCode;

				if (!proc.WaitForExit(TimeSpan.FromMinutes(5), out exitCode))
					throw new Exception("Process failed to end");

				var messages = proc.StdOut.ReadAllText(Encoding.UTF8);
				var errors = proc.StdErr.ReadAllText(Encoding.UTF8);

				if (exitCode != 0) LogOutput.Log.Error(messages);
				else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(messages);

				if (exitCode != 0) LogOutput.Log.Error(errors);
				else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(errors);

				if (stdOutErr != null) stdOutErr(messages, errors);

				return exitCode;
			}
		}
	}
}