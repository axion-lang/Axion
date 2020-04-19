using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Axion.Core;
using Axion.Core.Processing.Errors;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxParserTests : Tests {
        private static Unit ParseTestFile([CallerMemberName] string testName = null) {
            Unit source = MakeSourceFromFile(testName);
            Parse(source);
            return source;
        }

        private static void Parse(Unit source) {
            Compiler.Process(source, ProcessingMode.Transpilation, new ProcessingOptions("csharp"));
        }

        [Test]
        public void TestBisectAlgorithm() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestClassDef() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConditionalExpression() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConsole2048() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConsoleSnake() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestDecisionTree() {
            Unit source = ParseTestFile();
            Assert.AreEqual(4, source.Blames.Count);
        }

        [Test]
        public void TestDesignPatterns() {
            for (var i = 0; i < SourceFiles.Count; i++) {
                FileInfo file = SourceFiles[i];
                Unit source = Unit.FromFile(
                    file,
                    new FileInfo(OutPath + nameof(TestDesignPatterns) + i + TestExtension)
                );
                Compiler.Process(source, ProcessingMode.Parsing, ProcessingOptions.Debug);
                Assert.That(
                    source.Blames.All(
                        b => b.Severity != BlameSeverity.Error
                          || b.Message  == BlameType.ImpossibleToInferType.Description
                    ),
                    file.Name + ": Errors count > 0"
                );
                Assert.That(source.TokenStream.Tokens.Count > 0, file.Name + ": Tokens count == 0");
            }
        }

        [Test]
        public void TestDotExpressions() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestDoWhileMacro() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFastFib() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFizzBuzz() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFuncCalls() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFuncDef() {
            Unit source = ParseTestFile();
            Assert.AreEqual(15, source.Blames.Count);
        }

        [Test]
        public void TestHuffmanCompression() {
            Unit source = ParseTestFile();
            Assert.AreEqual(2, source.Blames.Count);
        }

        [Test]
        public void TestMatchMacro() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestMathExprParser() {
            Unit source = ParseTestFile();
            Assert.AreEqual(5, source.Blames.Count);
        }

        [Test]
        public void TestMisc() {
            Unit source = ParseTestFile();
            Assert.AreEqual(4, source.Blames.Count);
        }

        [Test]
        public void TestModuleDef() {
            Unit unit1 = MakeSourceFromCode("module ExampleModule: pass");
            Parse(unit1);
            Assert.That(unit1.Blames.Count == 0, $"unit1.Blames.Count == {unit1.Blames.Count}");
        }

        [Test]
        public void TestNestedForComprehension() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestObjectTreeFromList() {
            Unit source = ParseTestFile();
            Assert.AreEqual(2, source.Blames.Count);
        }

        [Test]
        public void TestWhileExpression() {
            Unit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}
