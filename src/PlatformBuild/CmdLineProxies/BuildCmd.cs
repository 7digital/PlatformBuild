using System;
using System.IO;
using PlatformBuild.FileSystem;
using PlatformBuild.LogOutput;
using PlatformBuild.Rules;

namespace PlatformBuild.CmdLineProxies
{
	public class BuildCmd : IBuildCmd
	{
		readonly IPatterns _patterns;
		readonly IFileSystem _files;

		public BuildCmd(IPatterns patterns, IFileSystem files)
		{
			_patterns = patterns;
			_files = files;
		}

		public int Build(FilePath buildPath)
		{
			var path = _files.GetFirstMatch(buildPath, _patterns.BuildPattern);
			if (path == null) return 0;

			var c = "/c " + string.Format(_patterns.BuildCmd, path.LastElement());

			return buildPath.Call("cmd", c);
		}

		public int RunSqlScripts(FilePath root, FilePath script)
		{
			/* Old, platform neutral way
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
			*/

			//sqlcmd.exe -i \"#{script}\" -S #{server} -E -o #{temp_log}

			// quick and dirty hack way:
			Log.Status("                           " + script.LastElement());
			return root.Call("sqlcmd.exe", " -i \"" + script.ToEnvironmentalPath() + "\" -S .\\SQLEXPRESS -E");
		}
	}
}