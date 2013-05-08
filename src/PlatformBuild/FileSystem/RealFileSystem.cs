using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PlatformBuild.FileSystem
{
	public class RealFileSystem : IFileSystem
	{
		public FilePath GetPlatformRoot()
		{
			var here = Assembly.GetExecutingAssembly().Location;

			while (!Directory.Exists(Path.Combine(here, ".git")))
			{
				var next = Path.Combine(here, "..");
				if (here == next) throw new Exception("Not in a Git build platform");
				here = next;
			}

			return new FilePath(here).Normalise();
		}

		public bool Exists(FilePath filePath)
		{
			return File.Exists(filePath.ToEnvironmentalPath())
				|| Directory.Exists(filePath.ToEnvironmentalPath());
		}

		public string[] Lines(FilePath path)
		{
			return
				File.ReadAllLines(path.ToEnvironmentalPath())
				.Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l)).ToArray();
		}


		public IEnumerable<FilePath> SortedDescendants(FilePath filePath, string pattern)
		{
			var list = Directory.GetFiles(filePath.ToEnvironmentalPath(), pattern, SearchOption.AllDirectories)
				.ToList();
			list.Sort();
			return list.Select(f => new FilePath(f));
		}

		public void DeletePath(FilePath path)
		{
			var epath = path.ToEnvironmentalPath();
			var gitPath = path.Navigate((FilePath)".git").ToEnvironmentalPath();

			if (Directory.Exists(gitPath)) Directory.Delete(gitPath, true);

			if (Directory.Exists(epath))
				Directory.Delete(epath, true);
			else
				File.Delete(epath);
		}

		public FilePath GetFirstMatch(FilePath root, string pattern)
		{
			var first = Directory.EnumerateFiles(root.ToEnvironmentalPath(), pattern, SearchOption.TopDirectoryOnly)
				.FirstOrDefault();

			if (string.IsNullOrEmpty(first)) return null;

			return (FilePath)first;
		}
	}
}