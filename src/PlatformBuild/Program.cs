using System;
using System.Linq;
using System.Reflection;
using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.LogOutput;
using PlatformBuild.Rules;

namespace PlatformBuild
{
	public class Program
	{
		static void Main(string[] args)
		{
            var start = DateTime.Now;

            if (args.Length > 0 && args[0].Contains("help"))
            {
                ShowHelp();
                return;
            }

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			if (args.Length > 0) Log.SetLevel(args[0]);
            else Log.SetLevel("status");

			var runDbs = !(args.Any(a => a == "no-databases"));

            var files = new RealFileSystem();
            var rules = new RuleFactory(files.GetPlatformRoot(), files);
            var deps = new DependencyManager(rules.GetRulePatterns());
            var git = new Git();
			var build = new BuildCmd();

			var thing = new Builder(files, git, deps, rules, build);

			thing.Prepare();
			thing.RunBuild(runDbs);

			var end = DateTime.Now;

			Console.WriteLine("Completed " + end + ", took " + (end - start));
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
			    Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Unhandled error: " + ex.GetType() + " " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                Console.ResetColor();
            } else Console.WriteLine("Unexpected error of unknown type: "+e.ExceptionObject.GetType());

            Environment.Exit(1);
		}

		static void ShowHelp()
		{
			Console.WriteLine(@"Platform build tool

First argument sets log level: (error, status, info, verbose). Default is status (2nd lowest)
To skip databases, use 'no-databases'");
		}
	}
}
