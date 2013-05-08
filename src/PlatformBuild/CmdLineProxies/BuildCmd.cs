using System.IO;
using PlatformBuild.LogOutput;

namespace PlatformBuild.CmdLineProxies
{
	public class BuildCmd : IBuildCmd
	{
		public int Build(FilePath buildPath)
		{
			var executablePath = buildPath.Append((FilePath) "Build.cmd").ToEnvironmentalPath();
			
			if (File.Exists(executablePath))
				return buildPath.CallInFolder("Build.cmd", "");

			Log.Warning(string.Format("No Build.cmd for {0}", buildPath.ToEnvironmentalPath()));
			return 0;
		}

		public int RunSqlScripts(FilePath root, FilePath script)
		{
            // todo: make this configurable:
            var buildPath = root.Navigate((FilePath)"build");
            var sqlInfo = buildPath.Navigate((FilePath)"sql.rule").ToEnvironmentalPath();

            if (!File.Exists(sqlInfo))
            {
	            Log.Verbose("*** Must have build/sql.rule file to get parallel sql ***");
                return 0;
            }

			var sqlParams = File.ReadAllText(sqlInfo).Trim();
			Log.Status("                           " + script.LastElement());
			return buildPath.CallInFolder("sqlfk.exe", sqlParams + " -i \"" + script.ToEnvironmentalPath() + "\"");
		}
	}
}