using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
	public class Builder_Databases
	{
		
		Builder _subject;
		IFileSystem _filesystem;
		IGit _git;
		IDependencyManager _depMgr;
		IRuleFactory _ruleFac;
		IModules _modules;
		IList<AutoResetEvent> _locks;
		IBuildCmd _buildCmd;

		[SetUp]
		public void builder()
		{
			_filesystem = Substitute.For<IFileSystem>();
			_filesystem.GetPlatformRoot().Returns(The.Root);

            var dbpath = The.Root.Navigate((FilePath)"group1/proj2/DatabaseScripts");
            _filesystem.Exists(null).ReturnsForAnyArgs(c=> c.Args()[0] as FilePath == dbpath);
            _filesystem.SortedDescendants(null,null).ReturnsForAnyArgs(new [] {(FilePath)"one",(FilePath)"two"});

			_modules = Substitute.For<IModules>();
			_modules.Paths.Returns(new[] {"group1/proj1", "group1/proj2", "group2/proj3"});
			_modules.Repos.Returns(new[] {"p1Repo", "p2Repo", "p3Repo"});
			_locks = new List<AutoResetEvent> { new AutoResetEvent(true), new AutoResetEvent(true), new AutoResetEvent(true) };
			_modules.CreateAndSetLocks().Returns(_locks);

			_git = Substitute.For<IGit>();
			_depMgr = Substitute.For<IDependencyManager>();
			_ruleFac = Substitute.For<IRuleFactory>();
			_ruleFac.GetModules().Returns(_modules);

			_buildCmd = Substitute.For<IBuildCmd>();

			_subject = new Builder(_filesystem, _git, _depMgr, _ruleFac, _buildCmd);
			_subject.Prepare();
			_subject.RebuildDatabases();
		}


        [Test]
        public void finds_all_database_folders ()
        {
            _filesystem.Received(1).Exists(The.Root.Navigate((FilePath)"group1/proj1/DatabaseScripts"));
            _filesystem.Received(1).Exists(The.Root.Navigate((FilePath)"group1/proj2/DatabaseScripts"));
            _filesystem.Received(1).Exists(The.Root.Navigate((FilePath)"group2/proj3/DatabaseScripts"));
        }

        [Test]
        public void finds_scripts_inside_folders()
        {
            _filesystem.Received(1).SortedDescendants(
                The.Root.Navigate((FilePath)"group1/proj2/DatabaseScripts"),
                "*.sql");
        }

        [Test]
        public void runs_all_found_scripts ()
        {
			_buildCmd.Received(1).RunSqlScript(The.Root, (FilePath)"one");
			_buildCmd.Received(1).RunSqlScript(The.Root, (FilePath)"two");
        }
	}
}
