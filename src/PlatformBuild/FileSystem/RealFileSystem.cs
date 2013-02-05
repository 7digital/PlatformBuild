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
	}
}