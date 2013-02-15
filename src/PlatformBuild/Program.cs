using System;
using System.Linq;
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

			if (args.Length > 0) Log.SetLevel(args[0]);
            else Log.SetLevel("status");

			var runDbs = !(args.Any(a => a == "no-databases"));

            var files = new RealFileSystem();
            var rules = new RuleFactory(files.GetPlatformRoot(), files);
            var deps = new DependencyManager(rules.GetRulePatterns());
            var git = new Git();
			var build = new BuildCmd();

			var thing = new Builder(files, git, deps, rules, build); 

            try
            {
	            thing.Prepare();
	            thing.RunBuild(runDbs);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Platform Build failed: " + ex.GetType() + " " + ex.Message + "\r\n\r\n" + ex.StackTrace);
                Console.ResetColor();
            }
			var end = DateTime.Now;

			Console.WriteLine("Completed " + end + ", took " + (end - start));
		}

		static void ShowHelp()
		{
			Console.WriteLine(@"Platform build tool

First argument sets log level: (status, info, verbose, error). Default is status (lowest)
To skip databases, use 'no-databases'");
		}
	}
}
