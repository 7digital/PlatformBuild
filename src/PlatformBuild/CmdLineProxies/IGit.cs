using System.IO;

namespace PlatformBuild.CmdLineProxies
{
	public interface IGit
	{
		void PullMaster(FilePath repoDir);
		void Clone(FilePath repoDir, FilePath filePath, string repo);
		void Reset(FilePath path);
		void PullCurrentBranch(FilePath modulePath, int times = 0);
	}
}
