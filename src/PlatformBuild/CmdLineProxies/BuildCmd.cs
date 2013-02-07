using System.IO;
using PlatformBuild.GitVCS;

namespace PlatformBuild
{
	public class BuildCmd : IBuildCmd
	{
		public int Build(FilePath buildPath)
		{
			return buildPath.Call("Build.cmd", "");
		}
	}
}