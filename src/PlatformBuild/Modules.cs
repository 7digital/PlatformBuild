using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlatformBuild.FileSystem;

namespace PlatformBuild
{
	public class Modules
	{
		readonly IFileSystem _files;
		public string[] Repos { get; private set; }
		public string[] Paths { get; private set; }
		public List<int>[] Deps { get; private set; }


		public Modules(FilePath filePath, IFileSystem files)
		{
			_files = files;
			ReadModules(filePath);
		}

		public void SortInDependencyOrder()
		{
			// go simple.
		}

		/// <summary>go find the build folder for each module.
		/// match in paths, write index to deps</summary>
		public void BuildDependencies(FilePath rootPath)
		{
			for (int i = 0; i < Paths.Length; i++)
			{
				var path = Paths[i];
				var brf = BuildPath(rootPath, path);
				if (!_files.Exists(brf)) continue;

				var lines = _files.Lines(brf);
				foreach (var line in lines)
				{
					Deps[i].Add(Paths.Index(line));
				}
			}
			
		}

		FilePath BuildPath(FilePath filePath, string path)
		{
			return 
				filePath.Navigate(
				new FilePath(path+"/Build/Depends.rule"))
				;
		}

		void ReadModules(FilePath filePath)
		{
			var lines = _files.Lines(filePath);

			var c = lines.Length;

			Repos = new string[c]; // src id => src repo
			Paths = new string[c]; // src id => file path
			Deps = new List<int>[c]; // src id => dst


			for (int i = 0; i < c; i++)
			{
				var bits = lines[i].Split('=').Select(s => s.Trim()).ToArray();

				Repos[i] = bits[1];
				Paths[i] = bits[0];
				Deps[i] = new List<int>();
			}
		}
	}
}