using System.IO;
using NSubstitute;
using NUnit.Framework;
using PlatformBuild;
using PlatformBuild.CmdLineProxies;
using PlatformBuild.DependencyManagement;
using PlatformBuild.FileSystem;
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
		FilePath _missingRepo;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);
			_missingRepo = The.Root.Append((FilePath)"/group1/proj2");

            _filesystem.Exists(null).ReturnsForAnyArgs(c=> (c.Args()[0] as FilePath !=_missingRepo));

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] {"group1/proj1", "group1/proj2", "group2/proj3"});
			_modules.Repos.Returns(new[] {"p1Repo", "p2Repo", "p3Repo"});

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac, null);
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
		public void get_modules_from_rules()
		{
			_ruleFac.Received().GetModules();
		}

        [Test]
        public void clones_missing_repos ()
        {
            _git.Received().Clone(The.Root, _missingRepo, "p2Repo");
        }

		[Test]
		public void sorts_dependencies()
		{
            _modules.Received().SortInDependencyOrder();
		}

		[Test]
		public void builds_pull_locks()
		{
			_modules.Received().CreateAndSetLocks();
		}
	}
}
