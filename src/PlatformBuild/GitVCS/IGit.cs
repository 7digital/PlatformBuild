using System.IO;
using System.Linq;

namespace PlatformBuild.GitVCS
{
	public interface IGit
	{
		void PullMaster(FilePath repoDir);
		void Clone(FilePath repoDir, FilePath filePath, string repo);
		void ResetLib(FilePath modulePath);
		void PullCurrentBranch(FilePath modulePath);
	}
}
