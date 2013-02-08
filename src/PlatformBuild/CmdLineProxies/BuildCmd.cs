using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public class BuildCmd : IBuildCmd
	{
		public int Build(FilePath buildPath)
		{
			return buildPath.Call("Build.cmd", "");
		}
	}
}