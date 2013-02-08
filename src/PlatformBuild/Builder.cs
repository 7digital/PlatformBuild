using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.Rules;

namespace PlatformBuild
{
	public class Builder
	{
		readonly IFileSystem _files;
		readonly IGit _git;
		readonly IDependencyManager _depMgr;
		readonly IRuleFactory _rules;
		readonly IBuildCmd _builder;
		FilePath _rootPath;
		public IModules Modules { get; private set; }
		IList<AutoResetEvent> _locks;

		public Builder(IFileSystem files, IGit git, IDependencyManager depMgr, IRuleFactory rules, IBuildCmd builder)
		{
			_files = files;
			_git = git;
			_depMgr = depMgr;
			_rules = rules;
			_builder = builder;
		}

		public void Prepare()
		{
			_rootPath = _files.GetPlatformRoot();
			Console.WriteLine("Started in "+_rootPath.ToEnvironmentalPath()+"; Updating self");

			_git.PullMaster(_rootPath);

            Modules = _rules.GetModules();
			Modules.ReadDependencies(_rootPath);
			Modules.SortInDependencyOrder();

			Console.WriteLine("Processing " + string.Join(", ", Modules.Paths));
			CloneMissingRepos();

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
				var modulePath = _rootPath.Navigate((FilePath)Modules.Paths[i]);
				var libPath = modulePath.Navigate((FilePath)"lib");

				var waitLock = _locks[i];
				
				if (_files.Exists(libPath)) _git.Reset(modulePath);
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

				CopyAvailableDependenciesToDirectory(libPath);

				if (!_files.Exists(buildPath)) continue;

				try
				{
					int code = _builder.Build(buildPath);
					if (code != 0) Console.WriteLine("Build error!");
					// todo : handle errors!
				}
				catch (Exception ex)
				{
					Cmd.Error(ex);
				}

				_depMgr.UpdateAvailableDependencies(srcPath);
			}
		}

        public void RebuildDatabases()
        {
			foreach (var path in Modules.Paths)
			{
				var dbPath = _rootPath.Navigate((FilePath)(path+"/DatabaseScripts"));
				if (!_files.Exists(dbPath)) continue;
				Console.WriteLine("Scripts from "+dbPath.ToEnvironmentalPath());

				foreach (FilePath file in _files.SortedDescendants(dbPath, "*.sql"))
				{
					Console.WriteLine(file.LastElement());
                    _builder.RunSqlScript(_rootPath, file);
					//_rootPath.Call("_build/Sqlfk.exe", file.ToEnvironmentalPath());
				}
			}
		}

		void CopyAvailableDependenciesToDirectory(FilePath moduleLibPath)
		{
			var dest = _rootPath.Navigate(moduleLibPath);
			_depMgr.CopyBuildResultsTo(dest);
		}

		void CloneMissingRepos()
		{
			for (int i= 0; i < Modules.Paths.Length; i++)
			{
				var path = new FilePath(Modules.Paths[i]);
				var expected = _rootPath.Navigate(path);
				if (_files.Exists(expected)) continue;

				Console.WriteLine(path.ToEnvironmentalPath() + " is missing. Cloning...");
				_git.Clone(_rootPath, expected, Modules.Repos[i]);
			}
		}
	}
}