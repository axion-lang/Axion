using System.IO;
using Axion.Core;
using Axion.Core.Processing.Source;
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
                Assert.That(source.Tokens.Count > 0,  file.Name + ": Tokens count == 0");
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
}