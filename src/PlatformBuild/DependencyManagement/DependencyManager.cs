using System.Collections.Generic;
using System.IO;
using PlatformBuild.Crap;
using PlatformBuild.LogOutput;
using PlatformBuild.Rules;

namespace PlatformBuild.DependencyManagement
{
	public class DependencyManager : IDependencyManager
	{
		readonly IPatterns _patterns;
		readonly Dictionary<string, FileInfo> _available;
        readonly Dictionary<string, FileInfo> _masters;

		public DependencyManager(IPatterns patterns)
		{
			_patterns = patterns;
			_available = new Dictionary<string, FileInfo>(); // name => info
			_masters = new Dictionary<string, FileInfo>(); // name => info
		}

		public void CopyBuildResultsTo(FilePath dest)
		{
			// For every file in dest, if we have a newer copy then overwrite dest file.
            // masters always override other available files
			if (!Directory.Exists(dest.ToEnvironmentalPath())) return;
            var files = Directory.GetFiles(dest.ToEnvironmentalPath(), _patterns.DependencyPattern, SearchOption.TopDirectoryOnly);
			foreach (var target in files)
			{
				var name = Path.GetFileName(target);
				if (name == null) continue;

                var source = _masters.Of(name) ?? _available.Of(name);

				if (source == null)
				{
				    Log.Verbose("No source for => " + target);
					continue;
				}

				Log.Verbose(_available[name].FullName + " => " + target);
				source.CopyTo(target, true);
			}
		}

		public void UpdateAvailableDependencies(FilePath srcPath)
		{
			if (!Directory.Exists(srcPath.ToEnvironmentalPath())) return;
			// for every pattern file under srcPath, add to list if it's the newest
			SetByLatest(srcPath, _available, _patterns.DependencyPattern);
		}

		static void SetByLatest(FilePath srcPath, IDictionary<string, FileInfo> repo, string pattern)
		{
			var files = Directory.GetFiles(srcPath.ToEnvironmentalPath(), pattern, SearchOption.AllDirectories);
			foreach (var file in files)
			{
				var name = Path.GetFileName(file);
				if (name == null) continue;
				var newInfo = new FileInfo(file);
				if (!repo.ContainsKey(name))
				{
					repo.Add(name, newInfo);
					continue;
				}

				var existing = repo[name];
				if (newInfo.LastWriteTime > existing.LastWriteTime)
					repo[name] = newInfo;
			}
		}

		public void ReadMasters(FilePath rootPath, FilePath[] masters)
		{
			foreach (var master in masters)
			{
				var fullPath = rootPath.Navigate(master);
			    if (!Directory.Exists(fullPath.ToEnvironmentalPath())) continue;

				SetByLatest(fullPath, _masters, "*");
			}
		}
	}
}