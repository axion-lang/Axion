using System.Runtime.CompilerServices;
using Axion.Core;
using Axion.Core.Hierarchy;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Parser {
    [TestFixture]
    public partial class SyntaxParserTests {
        private static Unit ParseTestFile(
            [CallerMemberName] string testName = null!
        ) {
            var file       = TestUtils.FileFromTestName(testName);
            var rootModule = Module.RawFrom(file.Directory!);
            rootModule.Bind(Unit.FromFile(TestUtils.StdLibMacrosFile));
            var unit = TestUtils.UnitFromFile(testName);
            rootModule.Bind(unit);
            Compiler.Process(rootModule, new ProcessingOptions(Mode.Parsing));
            return unit;
        }

        [Test]
        public void TestBisectAlgorithm() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestClassDef() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }
        
        [Test]
        public void TestClassDefGeneric() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestConditionalExpression() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestConsole2048() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestConsoleSnake() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestDecisionTree() {
            var unit = ParseTestFile();
            Assert.AreEqual(4, unit.Blames.Count);
        }

        [Test]
        public void TestDotExpressions() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestDoWhileMacro() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFastFib() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFizzBuzz() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFuncCalls() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFuncDefsSimple() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFuncDefsBraces() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestFuncDefsGeneric() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestHuffmanCompression() {
            var unit = ParseTestFile();
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void TestImports() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestMatchMacro() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestMathExprParser() {
            var unit = ParseTestFile();
            Assert.AreEqual(5, unit.Blames.Count);
        }

        [Test]
        public void TestMisc() {
            var unit = ParseTestFile();
            Assert.AreEqual(4, unit.Blames.Count);
        }

        [Test]
        public void TestNestedForComprehension() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }

        [Test]
        public void TestObjectTreeFromList() {
            var unit = ParseTestFile();
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void TestWhileExpression() {
            var unit = ParseTestFile();
            Assert.IsEmpty(unit.Blames);
        }
    }
}
