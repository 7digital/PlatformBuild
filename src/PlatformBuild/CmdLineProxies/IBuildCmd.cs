using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public interface IBuildCmd
	{
		int Build(FilePath buildPath);
        int RunSqlScript(FilePath root, FilePath script);
	}
}