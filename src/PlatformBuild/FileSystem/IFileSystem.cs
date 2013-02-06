using System.IO;

namespace PlatformBuild.FileSystem
{
	public interface IFileSystem
	{
		FilePath GetExeDirectory();
		bool Exists(FilePath filePath);
		string[] Lines(FilePath path);
	}
}