using System.IO;
using Axion.Core;
using Axion.Core.Processing.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    public partial class LexerTests {
        [Test]
        public void IsFail_UnclosedBlockComment() {
            string[] files = Directory.GetFiles(
                InPath,
                $"{nameof(IsFail_UnclosedBlockComment)}*{Compiler.SourceFileExtension}"
            );

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source =
                    MakeSourceFromFile(nameof(IsFail_UnclosedBlockComment) + "_" + i);
                Lex(source);
                Assert.That(source.Blames.Count == 1, $"{i} blames != 1");
            }
        }

        [Test]
        public void IsOK_ValidBlockComment() {
            string[] files = Directory.GetFiles(
                InPath,
                $"{nameof(IsOK_ValidBlockComment)}*{Compiler.SourceFileExtension}"
            );

            // validate
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source =
                    MakeSourceFromFile(nameof(IsOK_ValidBlockComment) + "_" + i);
                Lex(source);
                Assert.AreEqual(0, source.Blames.Count);
            }
        }
    }
}