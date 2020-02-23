using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestEscapedCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("tab = `\\t`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestFailEmptyCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = ``");
            Lex(source);
            // 1) empty
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestFailLongCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `abc`");
            Lex(source);
            // 1) too long
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestFailUnclosedCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `abcdef\n");
            Lex(source);
            Assert.AreEqual(1, source.Blames.Count);
        }

        [Test]
        public void TestSmileCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `✌`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void TestValidCharacterLiteral() {
            SourceUnit source = MakeSourceFromCode("x = `q`");
            Lex(source);
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}