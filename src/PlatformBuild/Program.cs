using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;
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

			var thing = new Builder(files, git, deps, rules); 

			thing.Prepare();
			thing.RunBuild();
		}
	}
}
