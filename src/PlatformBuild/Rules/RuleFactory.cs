using System.IO;
using PlatformBuild.FileSystem;

namespace PlatformBuild.Rules
{
	public class RuleFactory : IRuleFactory
	{
		readonly FilePath _rootDirectory;
		readonly IFileSystem _files;
		readonly FilePath _modulesPath;

		public RuleFactory(FilePath rootDirectory, IFileSystem files)
		{
			_rootDirectory = rootDirectory;
			_files = files;

			DependencyPattern = "*.dll"; //todo: actually read!
			_modulesPath = new FilePath("_rules/Modules.rule");
		}

		public string DependencyPattern { get; set; }
		public IModules GetModules()
		{
			var rulesFile = _rootDirectory.Navigate(_modulesPath);
			return  new Modules(rulesFile, _files);
		}
	}
}
