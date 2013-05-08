using System;
using System.Collections.Generic;
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
			var build = _rootDirectory.Navigate((FilePath)"_rules/BuildCommand.rule");

			if (!_files.Exists(libPath)) throw new Exception("_rules/DependencyPath.rule is missing");
			if (!_files.Exists(libPatt)) throw new Exception("_rules/DependencyPatterns.rule is missing");
			if (!_files.Exists(masters)) throw new Exception("_rules/Masters.rule is missing");
			if (!_files.Exists(build)) throw new Exception("_rules/BuildCommand.rule is missing");

			var bcmd = _files.Lines(build).First().Split('=');

			return new Patterns
			{
				DependencyPath = new FilePath(_files.Lines(libPath).First()),
				DependencyPattern = string.Join("|", _files.Lines(libPatt)),
				Masters = _files.Lines(masters).Select(str => (FilePath)str).ToArray(),
				BuildCmd = bcmd[1].Trim(),
				BuildPattern = bcmd[0].Trim()
			};
		}

		public IModules GetModules()
		{
			var rulesFile = _rootDirectory.Navigate(_modulesPath);
			return new Modules(rulesFile, _files, GetRulePatterns());
		}

		public IEnumerable<FilePath> GetPathsToDelete()
		{
			var clearRule = _rootDirectory.Navigate((FilePath)"_rules/PathsToDelete.rule");
			if (! _files.Exists(clearRule) ) yield break;
			var paths = _files.Lines(clearRule).Select(str => _rootDirectory.Navigate((FilePath)str));
			foreach (var path in paths) yield return path;
		}
	}
}
