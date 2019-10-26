using System.IO;
using Axion.Core;
using Axion.Core.Source;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        [Test]
        public void TestDesignPatterns() {
            for (var i = 0; i < SourceFiles.Count; i++) {
                FileInfo file = SourceFiles[i];
                SourceUnit source = SourceUnit.FromFile(
                    file,
                    new FileInfo(OutPath + nameof(TestDesignPatterns) + i + TestExtension)
                );
                Lex(source);
                Assert.That(source.Blames.Count             == 0, file.Name + ": Errors count > 0");
                Assert.That(source.TokenStream.Tokens.Count > 0,  file.Name + ": Tokens count == 0");
            }
        }

        private static void Lex(SourceUnit source) {
            Compiler.Process(
                source,
                ProcessingMode.Lex,
                ProcessingOptions.SyntaxAnalysisDebugOutput
            );
        }
    }
}