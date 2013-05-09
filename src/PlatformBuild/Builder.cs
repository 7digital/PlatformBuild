using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.LogOutput;
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
		IPatterns _patterns;

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
			Log.Status("Started in " + _rootPath.ToEnvironmentalPath() + "; Updating self");

			_git.PullMaster(_rootPath);

			_patterns = _rules.GetRulePatterns();
			Modules = _rules.GetModules();
			Modules.ReadDependencies(_rootPath);
			Modules.SortInDependencyOrder();

			_depMgr.ReadMasters(_rootPath, _patterns.Masters);

			DeleteOldPaths();

			Log.Verbose("Processing " + string.Join(", ", Modules.Paths));
			CloneMissingRepos();

			_locks = Modules.CreateAndSetLocks();
		}

		void DeleteOldPaths()
		{
			Log.Verbose("Deleting old paths");
			foreach(var path in _rules.GetPathsToDelete())
			{
				if (! _files.Exists(path)) continue;
					
				try
				{
					_files.DeletePath(path);
				} catch (Exception ex)
				{
					Log.Error("Failed to delete " + path.ToEnvironmentalPath() + ", because of a " + ex.GetType());
				}
			}
		}

		public void RunBuild(bool runDatabases)
		{
			Thread databases = null;
			if (runDatabases)
			{
				databases = new Thread(RebuildDatabases);
				databases.Start();
			}

			var pulling = new Thread(PullRepos);
			var building = new Thread(GetDependenciesAndBuild);

			pulling.Start();
			building.Start();

			building.Join();
			Log.Status("All builds finished");

			if (runDatabases)
			{
				databases.Join();
				Log.Status("All databases updated");
			}
		}

		public void PullRepos()
		{
			for (int i = 0; i < Modules.Paths.Length; i++)
			{
				var modulePath = _rootPath.Navigate((FilePath)Modules.Paths[i]);
				var libPath = modulePath.Navigate((FilePath)"lib");

				if (_files.Exists(libPath)) _git.CheckoutFolder(libPath);
				_git.PullCurrentBranch(modulePath);
				Log.Status("Updated " + Modules.Paths[i]);
				_locks[i].Set();
			}
		}

		public void GetDependenciesAndBuild()
		{
			for (int i = 0; i < Modules.Paths.Length; i++)
			{
				var moduleName = Modules.Paths[i];
				var buildPath = _rootPath.Navigate((FilePath)(moduleName));
				var libPath = _rootPath.Navigate((FilePath)(moduleName)).Navigate(_patterns.DependencyPath);
				var srcPath = _rootPath.Navigate((FilePath)(moduleName + "/src"));

				if (!_locks[i].WaitOne(TimeSpan.FromSeconds(1)))
				{
					Log.Info("Waiting for git update of " + moduleName);
					if (!_locks[i].WaitOne(TimeSpan.FromSeconds(30)))
					{
						Log.Error("Waiting a long time for " + moduleName + " to update!");
						_locks[i].WaitOne();
					}
				}

				CopyAvailableDependenciesToDirectory(libPath);

				if (!_files.Exists(buildPath))
				{
					Log.Info("Ignoring " + moduleName + " because it has no build folder");
					continue;
				}

				Log.Info("Starting build of " + moduleName);
				try
				{
					int code = _builder.Build(_rootPath, buildPath);
					if (code != 0) Log.Error("Build failed: " + moduleName);
					else Log.Status("Build complete: " + moduleName);
				}
				catch (Exception ex)
				{
					Log.Error("Build error: " + ex.GetType() + ": " + ex.Message);
				}

				_depMgr.UpdateAvailableDependencies(srcPath);
			}
		}

		public void RebuildDatabases()
		{
			foreach (var path in Modules.Paths)
			{
				var projPath = _rootPath.Navigate((FilePath)path);
				var dbPath = projPath.Navigate((FilePath)"DatabaseScripts");
				var sqlSpecificPath = dbPath.Navigate((FilePath)"SqlServer");

				if (!_files.Exists(dbPath)) continue;
				Log.Status("Scripts from " + dbPath.ToEnvironmentalPath());

				var finalSrcPath = (_files.Exists(sqlSpecificPath)) ? (sqlSpecificPath) : (dbPath);

				foreach (FilePath file in _files.SortedDescendants(finalSrcPath, "*.sql"))
				{
					Log.Verbose(file.LastElement());
					_builder.RunSqlScripts(projPath, file);
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
			for (int i = 0; i < Modules.Paths.Length; i++)
			{
				var path = new FilePath(Modules.Paths[i]);
				var expected = _rootPath.Navigate(path);
				if (_files.Exists(expected)) continue;

				Log.Info(path.ToEnvironmentalPath() + " is missing. Cloning...");
				_git.Clone(_rootPath, expected, Modules.Repos[i]);
			}
		}
	}
}