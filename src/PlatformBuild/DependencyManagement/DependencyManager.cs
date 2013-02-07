using System.Collections.Generic;
using System.IO;

namespace PlatformBuild.DependencyManagement
{
	public class DependencyManager : IDependencyManager
	{
		readonly string _matchingPattern;
		readonly Dictionary<string, FileInfo> _available;

		public DependencyManager(string matchingPattern)
		{
			_matchingPattern = matchingPattern;
			_available = new Dictionary<string, FileInfo>(); // name => info
		}

		public void CopyBuildResultsTo(FilePath dest)
		{
			// For every file in dest, if we have a newer copy then overwrite dest file.
            var files = Directory.GetFiles(dest.ToEnvironmentalPath(), _matchingPattern, SearchOption.TopDirectoryOnly);
			foreach (var target in files)
			{
				var name = Path.GetFileName(target);
				if (name == null) continue;
				if (!_available.ContainsKey(name)) continue;

				_available[name].CopyTo(target, true);
			}
		}

		public void UpdateAvailableDependencies(FilePath srcPath)
		{
			// for every pattern file under srcPath, add to list if it's the newest
			var files = Directory.GetFiles(srcPath.ToEnvironmentalPath(), _matchingPattern, SearchOption.AllDirectories);
			foreach (var file in files)
			{
                var name = Path.GetFileName(file);
                if (name == null) continue;
                var newInfo = new FileInfo(file);
				if (!_available.ContainsKey(name))
				{
                    _available.Add(name,newInfo);
                    continue;
				}

                var existing = _available[name];
                if (newInfo.LastWriteTime > existing.LastWriteTime)
                    _available[name] = newInfo;
			}
		}
	}
}