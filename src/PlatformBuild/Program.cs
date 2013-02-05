using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;

namespace PlatformBuild
{
	public class Program
	{
		static void Main()
		{
			new Builder(new RealFileSystem(), new Git()).Build();
		}
	}
}
