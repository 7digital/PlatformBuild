using System.IO;
using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;

namespace PlatformBuild
{
	public class Builder
	{
		readonly IFileSystem _files;
		readonly IGit _git;
		readonly FilePath _modulesPath = new FilePath("_rules/Modules.rule");
		FilePath _rootPath;

		public Builder(IFileSystem files, IGit git)
		{
			_files = files;
			_git = git;
		}

		public void Build()
		{
			_rootPath = _files.GetExeDirectory();
			var rulesFile = _rootPath.Navigate(_modulesPath);

			_git.PullMaster(_rootPath);

			// find & read modules file into modules class
			var modules = new Modules(rulesFile, _files);
			CloneMissingRepos(modules, _rootPath);
			modules.BuildDependencies(_rootPath);
		}

		void CloneMissingRepos(Modules modules, FilePath root)
		{
			for (int i= 0; i < modules.Paths.Length; i++)
			{
				var path = modules.Paths[i];
				var expected = root.Navigate(new FilePath(path));
				if (_files.Exists(expected)) continue;

				_git.Clone(_rootPath, expected, modules.Repos[i]);
			}
		}
	}
}