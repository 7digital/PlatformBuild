using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;

namespace PlatformBuild
{
	public class Program
	{
		static void Main()
		{
			var thing = new Builder(new RealFileSystem(), new Git());
			thing.Prepare();
			thing.RunBuild();
		}
	}
}
