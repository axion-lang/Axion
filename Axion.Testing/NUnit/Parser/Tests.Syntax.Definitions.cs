using Axion.Core.Processing.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    public partial class SyntaxParserTests {
        [Test]
        public void IsOK_ClassDef() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsOK_EnumDef() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsOK_FuncDef() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}