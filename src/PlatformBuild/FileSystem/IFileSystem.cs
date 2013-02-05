using System.IO;

namespace PlatformBuild.FileSystem
{
	public interface IFileSystem
	{
		FilePath GetExeDirectory();
	}
}