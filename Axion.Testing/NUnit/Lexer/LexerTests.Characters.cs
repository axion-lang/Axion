using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestEscapedCharacterLiteral() {
            Unit source = MakeSourceFromCode("tab = `\\t`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFailEmptyCharacterLiteral() {
            Unit source = MakeSourceFromCode("x = ``");
            Lex(source);
            // 1) empty
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestFailLongCharacterLiteral() {
            Unit source = MakeSourceFromCode("x = `abc`");
            Lex(source);
            // 1) too long
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestFailUnclosedCharacterLiteral() {
            Unit source = MakeSourceFromCode("x = `abcdef\n");
            Lex(source);
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestSmileCharacterLiteral() {
            Unit source = MakeSourceFromCode("x = `✌`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestValidCharacterLiteral() {
            Unit source = MakeSourceFromCode("x = `q`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}
