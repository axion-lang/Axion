using Axion.Core.Hierarchy;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestRegularCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("x = `q`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestEmojiCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("x = `✌`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestEscapedCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("tab = `\\t`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFailEmptyCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("x = ``");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }

        [Test]
        public void TestFailLongCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("x = `abc`");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }

        [Test]
        public void TestFailUnclosedCharacterLiteral() {
            Unit unit = TestUtils.UnitFromCode("x = `abcdef\n");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }
    }
}
