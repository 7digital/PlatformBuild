using System.Collections.Generic;
using System.IO;

namespace PlatformBuild.FileSystem
{
	public interface IFileSystem
	{
		FilePath GetPlatformRoot();
		bool Exists(FilePath filePath);
		string[] Lines(FilePath path);
		IEnumerable<FilePath> SortedDescendants(FilePath filePath, string pattern);
		void DeletePath(FilePath path);
		FilePath GetFirstMatch(FilePath root, string pattern);
	}
}