using System.IO;
using Axion.Core;
using Axion.Core.Hierarchy;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class ProjectTests {
        private static readonly string testProjectsPath =
            Path.Join(TestUtils.InPath, "projects");

        [Test]
        public void TestEmptyProject() {
            var proj = new Project(
                Path.Join(testProjectsPath, "Empty", "project.toml")
            );
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
    }
}
