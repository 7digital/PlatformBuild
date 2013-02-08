using System;
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

            if (args.Length > 0) Log.SetLevel(args[0]);
            else Log.SetLevel("status");

            var files = new RealFileSystem();
            var rules = new RuleFactory(files.GetPlatformRoot(), files);
            var deps = new DependencyManager(rules.GetRulePatterns());
            var git = new Git();
			var build = new BuildCmd();

			var thing = new Builder(files, git, deps, rules, build); 

			thing.Prepare();
			thing.RunBuild();

            var end = DateTime.Now;

			Console.WriteLine("Completed " + end + ", took " + (end - start));
		}
	}
}
