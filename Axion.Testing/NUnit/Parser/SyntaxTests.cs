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
        [Test]
        public void TestBisectAlgorithm() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }


        [Test]
        public void TestClassDef() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConditionalExpression() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConsole2048() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestConsoleSnake() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestDecisionTree() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(4, source.Blames.Count);
        }

        // [Test]
        // public void TestDecorators() {
        //     SourceUnit source = ParseTestFile();
        //     Assert.AreEqual(0, source.Blames.Count);
        // }

        [Test]
        public void TestDotExpressions() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestDoWhileMacro() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        // [Test]
        // public void TestEnumDef() {
        //     SourceUnit source = ParseTestFile();
        //     Assert.AreEqual(0, source.Blames.Count);
        // }
        
        [Test]
        public void TestDesignPatterns() {
            for (var i = 0; i < SourceFiles.Count; i++) {
                FileInfo file = SourceFiles[i];
                SourceUnit source = SourceUnit.FromFile(
                    file,
                    new FileInfo(OutPath + nameof(TestDesignPatterns) + i + TestExtension)
                );
                Compiler.Process(
                    source,
                    ProcessingMode.Parsing
                );
                Assert.That(source.Blames.All(b => b.Severity != BlameSeverity.Error), file.Name + ": Errors count > 0");
                Assert.That(source.TokenStream.Tokens.Count > 0,                       file.Name + ": Tokens count == 0");
            }
        }

        [Test]
        public void TestFastFib() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFizzBuzz() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFuncCalls() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFuncDef() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestHuffmanCompression() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(2, source.Blames.Count);
        }

        [Test]
        public void TestMacros() {
            SourceUnit source = MakeSourceFromFile(nameof(TestMacros));
            Compiler.Process(
                source,
                ProcessingMode.Parsing
            );
            Assert.AreEqual(7, source.Blames.Count);
        }

        [Test]
        public void TestMatchMacro() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestMathExprParser() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(5, source.Blames.Count);
        }

        [Test]
        public void TestMisc() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestNestedForComprehension() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestObjectTreeFromList() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(2, source.Blames.Count);
        }

        [Test]
        public void TestWhileExpression() {
            SourceUnit source = ParseTestFile();
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestModuleDef() {
            SourceUnit unit1 = MakeSourceFromCode("module ExampleModule: pass");
            Parse(unit1);
            Assert.That(unit1.Blames.Count == 0, $"unit1.Blames.Count == {unit1.Blames.Count}");
        }

        private static SourceUnit ParseTestFile(
            [CallerMemberName]
            string testName = null
        ) {
            SourceUnit source = MakeSourceFromFile(testName);
            Parse(source);
            return source;
        }

        private static void Parse(SourceUnit source) {
            Compiler.Process(
                source,
                ProcessingMode.ConvertAxion
            );
            Compiler.Process(
                source,
                ProcessingMode.ConvertCS
            );
            Compiler.Process(
                source,
                ProcessingMode.ConvertPy
            );
        }
    }
}