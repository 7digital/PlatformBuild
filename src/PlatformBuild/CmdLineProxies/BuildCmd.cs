using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public class BuildCmd : IBuildCmd
	{
		public int Build(FilePath buildPath)
		{
			return buildPath.Call("Build.cmd", "");
		}

		public int RunSqlScript(FilePath root, FilePath script)
		{
			return root.Call("_build/Sqlfk.exe", script.ToEnvironmentalPath());
		}
	}
}