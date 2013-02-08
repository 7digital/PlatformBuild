using System;
using System.IO;
using System.Linq;
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

			_modulesPath = new FilePath("_rules/Modules.rule");
		}

        public IPatterns GetRulePatterns()
        {
            var libPath = _rootDirectory.Navigate((FilePath)"_rules/DependencyPath.rule");
            var libPatt = _rootDirectory.Navigate((FilePath)"_rules/DependencyPatterns.rule");
            var masters = _rootDirectory.Navigate((FilePath)"_rules/Masters.rule");
            
            if (!_files.Exists(libPath)) throw new Exception("_rules/DependencyPath.rule is missing");
            if (!_files.Exists(libPatt)) throw new Exception("_rules/DependencyPatterns.rule is missing");
            if (!_files.Exists(masters)) throw new Exception("_rules/Masters.rule is missing");

            return new Patterns {
                DependencyPath = new FilePath(_files.Lines(libPath).First()),
                DependencyPattern = string.Join("|",_files.Lines(libPatt)),
                Masters = _files.Lines(masters).Select(str=>(FilePath)str).ToArray()
			};
        }

		public IModules GetModules()
		{
			var rulesFile = _rootDirectory.Navigate(_modulesPath);
			return new Modules(rulesFile, _files, GetRulePatterns());
		}
	}
}
