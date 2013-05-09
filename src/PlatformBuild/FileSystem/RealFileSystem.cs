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

		public void CopyDirectory(FilePath src, FilePath dst)
		{
			if (!DirectoryExists(src)) throw new Exception("Source was not a folder or doesn't exist");
			if (!DirectoryExists(dst)) throw new Exception("Destination was not a folder or doesn't exist");

			var target = dst.Append((FilePath)src.LastElement());
			if (IsFile(target)) throw new Exception("Destination already exists as a file");
			if (DirectoryExists(target)) DeletePath(target);

			DirectoryCopy(src.ToEnvironmentalPath(), target.ToEnvironmentalPath(), true);
		}

		public bool IsFile(FilePath target)
		{
			return File.Exists(target.ToEnvironmentalPath());
		}

		public bool DirectoryExists(FilePath p)
		{
			return Directory.Exists(p.ToEnvironmentalPath());
		}

		/// <summary>
		/// Actually recommended by MS, rather than fixing the base class library
		/// http://msdn.microsoft.com/en-us/library/bb762914.aspx
		/// </summary>
		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			var dir = new DirectoryInfo(sourceDirName);
			var dirs = dir.GetDirectories();

			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			if (!copySubDirs) return;
			foreach (var subdir in dirs)
			{
				var temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, true);
			}
		}
	}
}