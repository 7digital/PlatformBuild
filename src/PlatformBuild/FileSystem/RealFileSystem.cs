using System.IO;
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
			throw new System.NotImplementedException();
		}
	}
}