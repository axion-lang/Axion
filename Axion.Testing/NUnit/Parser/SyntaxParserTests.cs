using Axion.Core.Processing;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxParserTests : Tests {
        [Test]
        public void ClassDefValid() {
            SourceUnit source = MakeSourceFromFile(nameof(ClassDefValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void DotExpressionsValid() {
            SourceUnit source = MakeSourceFromFile(nameof(DotExpressionsValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void EnumDefValid() {
            SourceUnit source = MakeSourceFromFile(nameof(EnumDefValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void FuncDefValid() {
            SourceUnit source = MakeSourceFromFile(nameof(FuncDefValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IfStmtValid() {
            SourceUnit source = MakeSourceFromFile(nameof(IfStmtValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void WhileStmtValid() {
            SourceUnit source = MakeSourceFromFile(nameof(WhileStmtValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void WithStmtValid() {
            SourceUnit source = MakeSourceFromFile(nameof(WithStmtValid));
            source.Process(SourceProcessingMode.Parsing, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}