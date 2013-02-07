using System;
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
		public Modules Modules { get; private set; }
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

			Modules = new Modules(rulesFile, _files);
			CloneMissingRepos(Modules, _rootPath);

			Modules.ReadDependencies(_rootPath);
			Modules.SortInDependencyOrder();
			_locks = Modules.CreateAndSetLocks();
		}

		public void RunBuild()
		{
			var pulling = new Thread(PullRepos);
			var building = new Thread(GetDependenciesAndBuild);
			var databases = new Thread(RebuildDatabases);

			pulling.Start();
			building.Start();
			databases.Start();

            databases.Join();
			building.Join();
		}

		public void PullRepos()
		{
			for (int i = 0; i < Modules.Paths.Length; i++)
			{
				var modulePath = new FilePath(Modules.Paths[i]);
				var waitLock = _locks[i];
				_git.ResetLib(modulePath);
				_git.PullCurrentBranch(modulePath);
				waitLock.Set();
			}
		}

		public void GetDependenciesAndBuild()
		{
			for (int i = 0; i < Modules.Paths.Length; i++)
			{
				var moduleBuildPath = new FilePath(Modules.Paths[i] + "/build");
				var moduleLibPath = new FilePath(Modules.Paths[i] + "/lib");
				_locks[i].WaitOne();

				CopyDependencies(i, moduleLibPath);

				if (_files.Exists(moduleBuildPath))
				{
					int code = moduleBuildPath.Call("Build.cmd", "");
                    if (code != 0) Console.WriteLine("Build error!");
					// todo : handle errors!
				}
			}
		}

        public void RebuildDatabases()
        {

	        foreach (var path in Modules.Paths)
	        {
		        var dbPath = new FilePath(path+"/DatabaseScripts");
                if (!_files.Exists(dbPath)) continue;

                foreach (FilePath file in _files.SortedDescendants(dbPath, "*.sql"))
                {
                    _rootPath.Call("_build/Sqlfk.exe", file.ToEnvironmentalPath());
                }
	        }
        }

		void CopyDependencies(int i, FilePath moduleLibPath)
		{
			foreach (var idx in Modules.Deps[i])
			{
				var source = _rootPath.Navigate(new FilePath(Modules.Paths[idx] + "/src"));
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