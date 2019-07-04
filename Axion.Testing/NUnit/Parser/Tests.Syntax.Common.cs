using System.Runtime.CompilerServices;
using Axion.Core;
using Axion.Core.Processing.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxParserTests : Tests {
        [Test]
        public void IsOK_DotExpressions() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }
        
        [Test]
        public void IsOK_ConditionalExpression() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsOK_WhileExpression() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }
        
        [Test]
        public void IsOK_NestedForComprehension() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

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