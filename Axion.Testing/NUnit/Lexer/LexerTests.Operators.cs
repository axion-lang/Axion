using Axion.Core.Processing;
using Axion.Core.Processing.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsOK_MinusPartOfIdentifier() {
            const string id     = "ident-ifier-";
            SourceUnit   source = MakeSourceFromCode(id);
            Lex(source);
            // id, minus, EOC
            Assert.AreEqual(3, source.Tokens.Count);
            Assert.AreEqual("ident-ifier", source.Tokens[0].Value);
        }

        [Test]
        public void IsFail_MismatchedClosingBracket() {
            SourceUnit source = MakeSourceFromCode("}");
            Lex(source);
            // 1) mismatching bracket
            Assert.AreEqual(1, source.Blames.Count);
        }
    }
}