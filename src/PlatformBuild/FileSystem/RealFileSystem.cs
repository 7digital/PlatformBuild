using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PlatformBuild.FileSystem
{
	public class RealFileSystem : IFileSystem
	{
		public FilePath GetExeDirectory()
		{
			return new FilePath(Assembly.GetExecutingAssembly().Location);
		}

		public bool Exists(FilePath filePath)
		{
			return File.Exists(filePath.ToEnvironmentalPath())
				|| Directory.Exists(filePath.ToEnvironmentalPath());
		}

		public string[] Lines(FilePath path)
		{
			return File.ReadAllLines(path.ToEnvironmentalPath());
		}

		public void DeepCopyByPattern(FilePath source, FilePath dest, string pattern)
		{
			var destFiles = Directory.GetFiles(dest.ToEnvironmentalPath(), "*.dll", SearchOption.AllDirectories); // todo: get from rules


		}

		public IEnumerable<FilePath> SortedDescendants(FilePath filePath, string pattern)
		{
			var list = Directory.GetFiles(filePath.ToEnvironmentalPath(), pattern, SearchOption.AllDirectories)
				.ToList();
			list.Sort();
			return list.Select(f => new FilePath(f));
		}
	}
}