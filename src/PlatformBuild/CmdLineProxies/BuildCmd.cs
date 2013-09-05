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

		public int Build(FilePath rootPath, FilePath buildPath)
		{
			var path = _files.GetFirstMatch(buildPath, _patterns.BuildPattern);
			if (path == null) return 0;

			foreach (var copyPath in _patterns.CopyPaths)
			{
				var src = rootPath.Navigate(copyPath.Source);
				var dst = buildPath.Navigate(copyPath.Destination);
				if (_files.Exists(src))
					_files.CopyDirectory(src, dst);
			}

			var command = string.Format(_patterns.BuildCmd, path.LastElement());
			Log.Verbose(command);

			return buildPath.Call("cmd", "/c " + command);
		}

		public int RunSqlScripts(FilePath root, FilePath script)
		{
			Log.Status("                           " + script.LastElement());
			return root.Call("sqlcmd.exe", " -i \"" + script.ToEnvironmentalPath() + "\" -S .\\SQLEXPRESS -E");
		}
	}
}