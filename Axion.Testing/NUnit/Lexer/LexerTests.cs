using Axion.Core;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        private static void Lex(SourceUnit source) {
            Compiler.Process(source, ProcessingMode.Lexing);
        }

        [Test]
        public void TestDashIdentifier() {
            const string id     = "ident-ifier-";
            SourceUnit   source = MakeSourceFromCode(id);
            Lex(source);
            // id, minus, EOC
            Assert.AreEqual(3, source.TokenStream.Tokens.Count);
            Assert.AreEqual("ident-ifier", source.TokenStream.Tokens[0].Value);
            Assert.AreEqual("ident_ifier", source.TokenStream.Tokens[0].Content);
        }

        [Test]
        public void TestMismatchingClosingBracket() {
            SourceUnit source = MakeSourceFromCode("}");
            Lex(source);
            // mismatching bracket
            Assert.AreEqual(1, source.Blames.Count);
        }
    }
}
