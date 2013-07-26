using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public interface IBuildCmd
	{
		int Build(FilePath rootPath, FilePath projectPath);
		int RunSqlScripts(FilePath root, FilePath script);
	}
}