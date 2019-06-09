using System.Runtime.CompilerServices;
using Axion.Core;
using Axion.Core.Processing;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxTreeNodeTests : Tests {
        [Test]
        public void ClassDefValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void DotExpressionsValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void EnumDefValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void FuncDefValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void ConditionalExpressionValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void WhileExpressionValid() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

//        [Test]
//        public void WithStmtValid() {
//            SourceUnit source = ParseTestFile();
//            Assert.AreEqual(0, source.Blames.Count);
//        }

        private static SourceUnit ParseTestFile([CallerMemberName] string testName = null) {
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