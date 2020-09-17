using System.IO;
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
            FileInfo file       = TestUtils.FileFromTestName(testName);
            Module   rootModule = Module.RawFrom(file.Directory!);
            rootModule.Bind(Unit.FromFile(TestUtils.StdLibMacrosFile));
            Unit unit = TestUtils.UnitFromFile(testName);
            rootModule.Bind(unit);
            Compiler.Process(rootModule, new ProcessingOptions(Mode.Parsing));
            return unit;
        }

        [Test]
        public void TestBisectAlgorithm() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestClassDef() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestConditionalExpression() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestConsole2048() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestConsoleSnake() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestDecisionTree() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(4, unit.Blames.Count);
        }

        [Test]
        public void TestDotExpressions() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestDoWhileMacro() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFastFib() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFizzBuzz() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFuncCalls() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFuncDef() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(15, unit.Blames.Count);
        }

        [Test]
        public void TestHuffmanCompression() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void TestMatchMacro() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestMathExprParser() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(5, unit.Blames.Count);
        }

        [Test]
        public void TestMisc() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(4, unit.Blames.Count);
        }

        [Test]
        public void TestNestedForComprehension() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestObjectTreeFromList() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(2, unit.Blames.Count);
        }

        [Test]
        public void TestWhileExpression() {
            Unit unit = ParseTestFile();
            Assert.AreEqual(0, unit.Blames.Count);
        }
    }
}
