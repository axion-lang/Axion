using Axion.Core;
using Axion.Core.Hierarchy;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests {
        static void Lex(Unit unit) {
            Compiler.Process(
                unit,
                new ProcessingOptions(Mode.Lexing) {
                    Debug = true
                }
            );
        }

        [Test]
        public void TestDashIdentifier() {
            const string id = "ident-ifier-";

            var unit = TestUtils.UnitFromCode(id);
            Lex(unit);
            // id, minus, EOC
            Assert.AreEqual(3, unit.TokenStream.Count);
            Assert.AreEqual("ident-ifier", unit.TokenStream[0].Value);
            Assert.AreEqual("ident_ifier", unit.TokenStream[0].Content);
        }

        [Test]
        public void TestMismatchingClosingBracket() {
            var unit = TestUtils.UnitFromCode("}");
            Lex(unit);
            // mismatching bracket
            Assert.AreEqual(1, unit.Blames.Count);
        }
    }
}
