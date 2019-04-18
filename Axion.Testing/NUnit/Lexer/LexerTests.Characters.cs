using Axion.Core.Processing;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_ValidCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `q`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsOK_EscapedCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("tab = `\\t`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsOK_SmileCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `✌`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void IsFail_EmptyCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = ``");
            Lex(source);
            // 1) empty
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void IsFail_LongCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `abc`");
            Lex(source);
            // 1) too long
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void IsFail_UnclosedCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `abcdef\n");
            Lex(source);
            // 1) too long
            // 2) unclosed
            Assert.AreEqual(2, source.Blames.Count);
        }
    }
}