using System.IO;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        [Test]
        public void DesignPatternsValidation() {
            for (var i = 0; i < SourceFiles.Count; i++) {
                FileInfo file = SourceFiles[i];
                var source = new SourceUnit(
                    file,
                    OutPath + nameof(DesignPatternsValidation) + i + TestExtension
                );
                Compiler.Process(
                    source,
                    SourceProcessingMode.Lex,
                    SourceProcessingOptions.SyntaxAnalysisDebugOutput
                );
                Assert.That(source.Blames.Count == 0, file.Name + ": Errors count > 0");
                Assert.That(source.Tokens.Count > 0, file.Name + ": Tokens count == 0");
            }
        }

        [Test]
        public void NestedMultilineCommentInvalid() {
            string[] files = Directory.GetFiles(
                InPath,
                $"{nameof(NestedMultilineCommentInvalid)}*{Spec.SourceFileExtension}"
            );

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source =
                    MakeSourceFromFile(nameof(NestedMultilineCommentInvalid) + "_" + i);
                Compiler.Process(
                    source,
                    SourceProcessingMode.Lex,
                    SourceProcessingOptions.SyntaxAnalysisDebugOutput
                );
                Assert.AreEqual(1, source.Blames.Count);
            }
        }

        [Test]
        public void NestedMultilineCommentValid() {
            string[] files = Directory.GetFiles(
                InPath,
                $"{nameof(NestedMultilineCommentValid)}*{Spec.SourceFileExtension}"
            );

            // validate
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source =
                    MakeSourceFromFile(nameof(NestedMultilineCommentValid) + "_" + i);
                Compiler.Process(
                    source,
                    SourceProcessingMode.Lex,
                    SourceProcessingOptions.SyntaxAnalysisDebugOutput
                );
                Assert.AreEqual(0, source.Blames.Count);
            }
        }

        [Test]
        public void StringsValidation() {
            SourceUnit source = MakeSourceFromFile(nameof(StringsValidation));
            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
            Assert.AreEqual(6, source.Blames.Count);
        }

        [Test]
        public void VariousStuffValid() {
            SourceUnit source = MakeSourceFromFile(nameof(VariousStuffValid));
            Compiler.Process(
                source,
                SourceProcessingMode.Lex,
                SourceProcessingOptions.SyntaxAnalysisDebugOutput
            );
            Assert.AreEqual(0, source.Blames.Count);
        }
    }
}