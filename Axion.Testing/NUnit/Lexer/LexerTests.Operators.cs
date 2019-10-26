using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void TestMinusPartOfIdentifier() {
            const string id     = "ident-ifier-";
            SourceUnit   source = MakeSourceFromCode(id);
            Lex(source);
            // id, minus, EOC
            Assert.AreEqual(3,             source.TokenStream.Tokens.Count);
            Assert.AreEqual("ident_ifier", source.TokenStream.Tokens[0].Value);
        }

        [Test]
        public void TestMismatchedClosingBracket() {
            SourceUnit source = MakeSourceFromCode("}");
            Lex(source);
            // 1) mismatching bracket
            Assert.AreEqual(1, source.Blames.Count);
        }
    }
}