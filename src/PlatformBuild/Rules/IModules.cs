using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PlatformBuild
{
	public interface IModules
	{
		string[] Repos { get; set; }
		string[] Paths { get; set; }
		List<int>[] Deps { get; set; }

		/// <summary> (1)
		/// go find the build folder for each module.
		/// match in paths, write index to deps
		/// </summary>
		void ReadDependencies(FilePath rootPath);

		/// <summary> (2)
		/// put modules in order, so we can build and distribute without
		/// getting any out-of-date libraries
		/// </summary>
		void SortInDependencyOrder();

		/// <summary> (3)
		/// Build a set of locks so builds don't get ahead of git-pulls
		/// </summary>
		IList<AutoResetEvent> CreateAndSetLocks();
	}
}