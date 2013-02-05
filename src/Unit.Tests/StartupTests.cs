using System.IO;
using NSubstitute;
using NUnit.Framework;
using PlatformBuild;
using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;

namespace Unit.Tests
{
	[TestFixture]
	public class StartupTests
	{
		Builder _subject;
		IFileSystem _filesystem;
		IGit _git;

		[SetUp]
		public void when_building ()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetExeDirectory().Returns(A.ExeDirectory);

			_git = Substitute.For<IGit>();
			_git.ParentRepo(Arg.Any<FilePath>()).Returns(A.ParentRepoDir);
			
			_subject = new Builder(_filesystem, _git);
			_subject.Build();
		}

		[Test]
		public void gets_its_own_directory_and_finds_repo()
		{
			_filesystem.Received().GetExeDirectory();
			_git.Received().ParentRepo(A.ExeDirectory);
		}

		[Test]
		public void pulls_the_current_branch_of_the_base_repo()
		{
			_git.Received().PullCurrent(A.ParentRepoDir);
		}
	}
}
