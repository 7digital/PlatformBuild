using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;
using PlatformBuild.Rules;

namespace PlatformBuild
{
	public class Builder
	{
		readonly IFileSystem _files;
		readonly IGit _git;
		readonly IDependencyManager _depMgr;
		readonly IRuleFactory _rules;
		FilePath _rootPath;
		public IModules Modules { get; private set; }
		IList<AutoResetEvent> _locks;

		public Builder(IFileSystem files, IGit git, IDependencyManager depMgr, IRuleFactory rules)
		{
			_files = files;
			_git = git;
			_depMgr = depMgr;
			_rules = rules;
		}

		public void Prepare()
		{
			_rootPath = _files.GetPlatformRoot();

			_git.PullMaster(_rootPath);

            Modules = _rules.GetModules();
			CloneMissingRepos();

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
				var buildPath = _rootPath.Navigate((FilePath)(Modules.Paths[i] + "/build"));
				var libPath = _rootPath.Navigate((FilePath)(Modules.Paths[i] + "/lib"));
				var srcPath = _rootPath.Navigate((FilePath)(Modules.Paths[i] + "/src"));
				_locks[i].WaitOne();

				CopyDependencies(libPath);

				if (!_files.Exists(buildPath)) continue;

				int code = buildPath.Call("Build.cmd", "");
				if (code != 0) Console.WriteLine("Build error!");
				// todo : handle errors!

				_depMgr.UpdateAvailableDependencies(srcPath);
			}
		}

        public void RebuildDatabases()
        {

			foreach (var path in Modules.Paths)
			{
				var dbPath = new FilePath(path + "/DatabaseScripts");
				if (!_files.Exists(dbPath)) continue;

				foreach (FilePath file in _files.SortedDescendants(dbPath, "*.sql"))
				{
					_rootPath.Call("_build/Sqlfk.exe", file.ToEnvironmentalPath());
				}
			}
		}

		void CopyDependencies(FilePath moduleLibPath)
		{
			var dest = _rootPath.Navigate(moduleLibPath);
			_depMgr.CopyBuildResultsTo(dest);
		}

		void CloneMissingRepos()
		{
			for (int i= 0; i < Modules.Paths.Length; i++)
			{
				var path = Modules.Paths[i];
				var expected = _rootPath.Navigate(new FilePath(path));
				if (_files.Exists(expected)) continue;

				_git.Clone(_rootPath, expected, Modules.Repos[i]);
			}
		}
	}
}