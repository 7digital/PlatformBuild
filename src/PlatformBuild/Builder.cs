using System.Collections.Generic;
using System.IO;
using System.Threading;
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
		Modules _modules;
		IList<AutoResetEvent> _locks;

		public Builder(IFileSystem files, IGit git)
		{
			_files = files;
			_git = git;
		}

		public void Prepare()
		{
			_rootPath = _files.GetExeDirectory();
			var rulesFile = _rootPath.Navigate(_modulesPath);

			_git.PullMaster(_rootPath);

			_modules = new Modules(rulesFile, _files);
			CloneMissingRepos(_modules, _rootPath);

			_modules.ReadDependencies(_rootPath);
			_modules.SortInDependencyOrder();
			_locks = _modules.CreateAndSetLocks();
		}

		public void RunBuild()
		{
			var pulling = new Thread(PullRepos);
			var building = new Thread(()=>GetDependenciesAndBuild(true));

			pulling.Start();
			building.Start();

			building.Join();
		}

		public void PullRepos()
		{
			for (int i = 0; i < _modules.Paths.Length; i++)
			{
				var modulePath = new FilePath(_modules.Paths[i]);
				var waitLock = _locks[i];
				_git.ResetLib(modulePath);
				_git.PullCurrentBranch(modulePath);
				waitLock.Set();
			}
		}

		public void GetDependenciesAndBuild(bool wait)
		{
			for (int i = 0; i < _modules.Paths.Length; i++)
			{
				var moduleBuildPath = new FilePath(_modules.Paths[i] + "/build");
				var moduleLibPath = new FilePath(_modules.Paths[i] + "/lib");
				var waitLock = _locks[i];
				if (wait) waitLock.WaitOne();

				CopyDependencies(i, moduleLibPath);

				if (_files.Exists(moduleBuildPath))
				{
					int code = moduleBuildPath.Call("Build.cmd", "");
					// todo : handle errors!
				}
			}
		}

		void CopyDependencies(int i, FilePath moduleLibPath)
		{
			foreach (var idx in _modules.Deps[i])
			{
				var source = _rootPath.Navigate(new FilePath(_modules.Paths[idx] + "/src"));
				var dest = _rootPath.Navigate(moduleLibPath);

				const string pattern = "*.dll"; // todo: get this from rules file!

				_files.DeepCopyByPattern(source, dest, pattern);
			}
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