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

		public int Build(FilePath rootPath, FilePath projectPath)
		{
			var path = _files.GetFirstMatch(projectPath, _patterns.BuildPattern);
			if (path == null) return 0;

			_files.CopyDirectory(
				rootPath.Navigate((FilePath)"_build"),
				projectPath.Navigate((FilePath)"build"));

			_files.Move(
				projectPath.Navigate((FilePath)"build/Rakefile"),
				projectPath.Navigate((FilePath)"Rakefile")); //(FilePath)"Rakefile")

			var command = string.Format(_patterns.BuildCmd, path.LastElement());
			Log.Verbose(command);

			return projectPath.Call("cmd", "/c " + command);
		}

		public int RunSqlScripts(FilePath root, FilePath script)
		{
			Log.Status("                           " + script.LastElement());
			return root.Call("sqlcmd.exe", " -i \"" + script.ToEnvironmentalPath() + "\" -S .\\SQLEXPRESS -E");
		}
	}
}