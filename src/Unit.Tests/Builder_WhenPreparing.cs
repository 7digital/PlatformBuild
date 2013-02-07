using System.IO;
using NSubstitute;
using NUnit.Framework;
using PlatformBuild;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
using PlatformBuild.GitVCS;
using PlatformBuild.Rules;

namespace Unit.Tests
{
	[TestFixture]
	public class Builder_WhenPreparing
	{
		Builder _subject;
		IFileSystem _filesystem;
		IGit _git;
		IDependencyManager _depMgr;
		IRuleFactory _ruleFac;
		IModules _modules;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);

            _filesystem.Exists(null).Returns(c=> (c.Args()[0] != (FilePath)"group1/proj2"));

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] {"group1/proj1", "group1/proj2", "group2/proj3"});
			_modules.Repos.Returns(new[] {"p1Repo", "p2Repo", "p3Repo"});

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac);
			_subject.Prepare();
		}

		[Test]
		public void gets_its_own_directory_and_finds_repo()
		{
			_filesystem.Received().GetPlatformRoot();
		}

		[Test]
		public void pulls_the_current_branch_of_the_base_repo()
		{
			_git.Received().PullMaster(The.Root);
		}

        [Test]
        public void pulls_missing_repos ()
        {
            var missingPath = The.Root.Append((FilePath)"/group1/proj2");
            var m = Arg.Is<FilePath>(fp => fp.ToPosixPath() == missingPath.ToPosixPath());
            _git.Received().Clone(The.Root, m, "p2Repo");
        }

		[Test]
		public void get_modules_from_rules()
		{
			_ruleFac.Received().GetModules();
		}

		[Test]
		public void sorts_dependencies()
		{
            _modules.Received().SortInDependencyOrder();
		}

	}
}
