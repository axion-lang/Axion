using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestRegularCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("x = `q`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestEmojiCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("x = `✌`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestEscapedCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("tab = `\\t`");
            Lex(unit);
            Assert.AreEqual(0, unit.Blames.Count);
        }

        [Test]
        public void TestFailEmptyCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("x = ``");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }

        [Test]
        public void TestFailLongCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("x = `abc`");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }

        [Test]
        public void TestFailUnclosedCharacterLiteral() {
            var unit = TestUtils.UnitFromCode("x = `abcdef\n");
            Lex(unit);
            Assert.AreEqual(1, unit.Blames.Count);
        }
    }
}
