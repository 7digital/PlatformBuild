using System.IO;

namespace PlatformBuild.FileSystem
{
	public interface IFileSystem
	{
		FilePath GetExeDirectory();
		bool Exists(FilePath filePath);
		string[] Lines(FilePath path);
		void DeepCopyByPattern(FilePath source, FilePath dest, string pattern);
	}
}