using System;
using System.Diagnostics;
using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public static class Cmd
	{
        static readonly object _writeLock = new Object();

		public static int Call(this FilePath root, string exe, string args)
		{
			var isBatch = exe.EndsWith(".cmd") || exe.EndsWith(".bat");

			var bin = (isBatch) ? ("cmd") : (exe);
			var rargs = (isBatch) ? ("/c " + exe + " " + args) : args;
			Console.WriteLine(root.Navigate((FilePath)exe).ToEnvironmentalPath() + " " + args);
			var proc = Process.Start(new ProcessStartInfo
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
			});

			proc.WaitForExit();

			WriteRed(proc.StandardError.ReadToEnd());
			WriteGrey(proc.StandardOutput.ReadToEnd());

			return proc.ExitCode;
		}

		static void WriteGrey(string str)
		{
			lock (_writeLock)
			{
                var old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine(str);
                Console.ForegroundColor = old;
			}
		}

		static void WriteRed(string str)
		{
			lock (_writeLock)
			{
                var old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(str);
                Console.ForegroundColor = old;
			}
		}

		public static void Error(Exception exception)
		{
			WriteRed(exception.GetType() + ": " + exception.Message);
		}
	}
}