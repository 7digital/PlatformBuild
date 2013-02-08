using System;
using System.Diagnostics;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public static class Cmd
	{
        static readonly object _writeLock = new Object();

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

			WriteGrey(root.ToEnvironmentalPath()+":"+bin + " " + rargs);

			var proc = Process.Start(processStartInfo);

			proc.WaitForExit();

            // TODO: read before exit?
			WriteRed(proc.StandardError.ReadToEnd());
			WriteGrey(proc.StandardOutput.ReadToEnd());

			return proc.ExitCode;
		}

		static void WriteGrey(string str)
		{
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine(str);
                Console.ResetColor();
			}
		}

		static void WriteRed(string str)
		{
			lock (_writeLock)
			{
                Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(str);
                Console.ResetColor();
			}
		}

		public static void Error(Exception exception)
		{
			WriteRed(exception.GetType() + ": " + exception.Message);
		}
	}
}