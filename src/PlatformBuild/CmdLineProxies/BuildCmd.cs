using System;
using System.IO;
using PlatformBuild.LogOutput;

namespace PlatformBuild.CmdLineProxies
{
	public class BuildCmd : IBuildCmd
	{
		public int Build(FilePath buildPath)
		{
			return buildPath.Call("Build.cmd", "");
		}

		public int RunSqlScripts(FilePath root, FilePath script)
		{
            // todo: make this configurable:
            var buildPath = root.Navigate((FilePath)"build");
            var sqlInfo = buildPath.Navigate((FilePath)"sql.rule").ToEnvironmentalPath();

            if (!File.Exists(sqlInfo))
            {
	            Log.Status("*** Must have build/sql.rule file to get parallel sql ***");
                return 0;
            }

			var sqlParams = File.ReadAllText(sqlInfo).Trim();
			return buildPath.CallInFolder("sqlfk.exe", sqlParams + " -i \"" + script.ToEnvironmentalPath()+"\"");
		}
	}
}