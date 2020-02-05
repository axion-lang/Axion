using Axion.Core;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        private static void Lex(SourceUnit source) {
            Compiler.Process(source, ProcessingMode.Lexing);
        }
    }
}