using System;
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

		public static int Call(this FilePath root, string exe, string args)
		{
            using (var process = new ProcessHost(exe, root.ToEnvironmentalPath()))
            {
				var callDescription = root.ToEnvironmentalPath() + ":" + exe + " " + args;
				LogOutput.Log.Verbose(callDescription);

                process.Start(args);

                int exitCode;
				if (!process.WaitForExit(TimeSpan.FromSeconds(10), out exitCode))
				{
					LogOutput.Log.Error("Call taking a long time: " + callDescription);
					if (!process.WaitForExit(TimeSpan.FromSeconds(60), out exitCode))
					{
						LogOutput.Log.Error("ABORTING LONG CALL " + callDescription);
						process.Kill();
                        exitCode = 127;
					}
				}

				var messages = process.StdOut.ReadAllText(Encoding.Default);
				if (exitCode != 0) LogOutput.Log.Error(messages);
				else if (!string.IsNullOrWhiteSpace(messages)) LogOutput.Log.Info(messages);

				var errors = process.StdErr.ReadAllText(Encoding.Default);
				if (!string.IsNullOrWhiteSpace(errors)) LogOutput.Log.Error(errors);

                return exitCode;
			}
		}
	}
}