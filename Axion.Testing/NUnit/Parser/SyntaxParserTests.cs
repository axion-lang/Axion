using Axion.Core;
using Axion.Core.Processing;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxTreeNodeTests : Tests {
        [Test]
        public void ClassDefValid() {
            SourceUnit source = ParseTestFile(nameof(ClassDefValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void DotExpressionsValid() {
            SourceUnit source = ParseTestFile(nameof(DotExpressionsValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void EnumDefValid() {
            SourceUnit source = ParseTestFile(nameof(EnumDefValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void FuncDefValid() {
            SourceUnit source = ParseTestFile(nameof(FuncDefValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IfStmtValid() {
            SourceUnit source = ParseTestFile(nameof(IfStmtValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void WhileStmtValid() {
            SourceUnit source = ParseTestFile(nameof(WhileStmtValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void WithStmtValid() {
            SourceUnit source = ParseTestFile(nameof(WithStmtValid));
            Assert.AreEqual(0, source.Blames.Count);
        }

        private SourceUnit ParseTestFile(string testName) {
            SourceUnit source = MakeSourceFromFile(testName);
            Parse(source);
            return source;
        }

        private static void Parse(SourceUnit source) {
            Compiler.Process(
                source,
                SourceProcessingMode.Parsing,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
        }
    }
}