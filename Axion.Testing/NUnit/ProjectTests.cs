using System.IO;
using Axion.Core;
using Axion.Core.Hierarchy;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class ProjectTests {
        private static readonly string testProjectsPath = Path.Join(
            TestUtils.InPath,
            "projects"
        );

        [Test]
        public void TestFilesHierarchy() {
            var proj = new Project(
                Path.Join(testProjectsPath, "FilesHierarchy", "project.toml")
            );
            Assert.AreEqual(
                Path.Join(
                    testProjectsPath,
                    "FilesHierarchy",
                    "out",
                    "Package1"
                ),
                proj.MainModule.Submodules["package1"]
                    .Units["Class1"]
                    .OutputDirectory.FullName
            );
            Assert.AreEqual(
                Path.Join(
                    testProjectsPath,
                    "FilesHierarchy",
                    "out",
                    "Package2"
                ),
                proj.MainModule.Submodules["package2"]
                    .Units["Class2"]
                    .OutputDirectory.FullName
            );
        }

        [Test]
        public void TestEmptyProject() {
            var proj = new Project(Path.Join(testProjectsPath, "Empty", "project.toml"));
            Assert.AreEqual(1, proj.ImportedMacros.Count);
            Assert.AreEqual("./macros.ax", proj.ImportedMacros[0]);
        }

        [Test]
        public void TestEmptyProjectWithStdLib() {
            var proj = new Project(
                Path.Join(testProjectsPath, "EmptyWithStdLib", "project.toml")
            );
            Assert.AreEqual(1, proj.MainModule.Submodules.Count);
            var stdlibModule = proj.MainModule.Submodules["stdlib"];
            Assert.AreEqual(1, stdlibModule.Units.Count);
            Unit stdMacrosUnit = stdlibModule.Units["macros"];

            Compiler.Process(proj, new ProcessingOptions(Mode.Parsing));

            Assert.IsFalse(stdMacrosUnit.HasErrors);
            Assert.AreEqual(13, stdlibModule.Definitions.Count);
        }

        [Test]
        public void TestProjectWithBasicImport() {
            var proj = new Project(
                Path.Join(testProjectsPath, "BasicImport", "project.toml")
            );
            Assert.AreEqual(3, proj.MainModule.Submodules.Count);
            var stdlibModule = proj.MainModule.Submodules["stdlib"];
            Assert.AreEqual(1, stdlibModule.Units.Count);

            Compiler.Process(proj, new ProcessingOptions(Mode.Parsing));

            Assert.AreEqual(13, stdlibModule.Definitions.Count);
        }
    }
}
