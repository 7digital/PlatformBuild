using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public interface IBuildCmd
	{
		int Build(FilePath rootPath, FilePath buildPath);
		int RunSqlScripts(FilePath root, FilePath script);
	}
}