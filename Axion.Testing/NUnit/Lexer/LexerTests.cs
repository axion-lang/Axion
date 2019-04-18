using System.IO;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Lexical.Tokens;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        [Test]
        public void IsOK_DesignPatterns() {
            for (var i = 0; i < SourceFiles.Count; i++) {
                FileInfo file = SourceFiles[i];
                var source = new SourceUnit(
                    file,
                    OutPath + nameof(IsOK_DesignPatterns) + i + TestExtension
                );
                Lex(source);
                Assert.That(source.Blames.Count == 0, file.Name + ": Errors count > 0");
                Assert.That(source.Tokens.Count > 0, file.Name + ": Tokens count == 0");
            }
        }

        private static void Lex(SourceUnit source) {
            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
        }
    }

    internal static class TokenUtils {
        internal static bool TokenEquals(this Token a, Token b) {
            return a.Is(b.Type)
                   && string.Equals(a.Value, b.Value)
                   && string.Equals(a.EndWhitespaces, b.EndWhitespaces);
        }

        internal static bool TestEquality(this NumberOptions t, NumberOptions other) {
            // don't check value equality
            return t.Radix == other.Radix
                   && t.Bits == other.Bits
                   && t.Floating == other.Floating
                   && t.Imaginary == other.Imaginary
                   && t.Unsigned == other.Unsigned
                   && t.Unlimited == other.Unlimited
                   && t.HasExponent == other.HasExponent
                   && t.Exponent == other.Exponent;
        }
    }
}