using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.Rules;

namespace PlatformBuild
{
	public class Program
	{
		static void Main()
		{
            var files = new RealFileSystem();
            var rules = new RuleFactory(files.GetPlatformRoot(), files);
            var deps = new DependencyManager(rules.DependencyPattern);
            var git = new Git();
			var build = new BuildCmd();

			var thing = new Builder(files, git, deps, rules, build); 

			thing.Prepare();
			thing.RunBuild();
		}
	}
}
