using System.IO;
using Axion.Core.Processing;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer {
    [TestFixture]
    public partial class LexerTests : Tests {
        [Test]
        public void NestedMultilineCommentInvalid() {
            string[] files = Directory.GetFiles(
                inPath,
                $"{nameof(NestedMultilineCommentInvalid)}*{Spec.SourceFileExtension}"
            );

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source = MakeSourceFromFile(nameof(NestedMultilineCommentInvalid) + "_" + i);
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.AreEqual(1, source.Blames.Count);
            }
        }

        [Test]
        public void NestedMultilineCommentValid() {
            string[] files = Directory.GetFiles(
                inPath,
                $"{nameof(NestedMultilineCommentValid)}*{Spec.SourceFileExtension}"
            );

            // validate
            for (var i = 1; i < files.Length + 1; i++) {
                SourceUnit source = MakeSourceFromFile(nameof(NestedMultilineCommentValid) + "_" + i);
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.AreEqual(0, source.Blames.Count);
            }
        }

        [Test]
        public void StringsValidation() {
            SourceUnit source = MakeSourceFromFile(nameof(StringsValidation));
            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(6, source.Blames.Count);
        }

        [Test]
        public void VariousStuffValid() {
            SourceUnit source = MakeSourceFromFile(nameof(VariousStuffValid));
            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void DesignPatternsValidation() {
            for (var i = 0; i < sourceFiles.Count; i++) {
                FileInfo file = sourceFiles[i];
                var source = new SourceUnit(
                    file,
                    outPath + nameof(DesignPatternsValidation) + i + testExtension
                );
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.That(source.Blames.Count == 0, file.Name + ": Errors count > 0");
                Assert.That(source.Tokens.Count > 0,  file.Name + ": Tokens count == 0");
            }
        }
    }
}