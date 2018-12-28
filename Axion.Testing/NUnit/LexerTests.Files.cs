using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public partial class LexerTests {
        #region Define file paths

        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory)
                .Parent.Parent.Parent;

        private readonly string __samplesPath =
            axionTestingDir.Parent.FullName + "\\Other\\Code Examples\\";

        private string samplesPath {
            get {
                if (!Directory.Exists(__samplesPath)) {
                    Directory.CreateDirectory(__samplesPath);
                }
                return __samplesPath;
            }
        }

        private readonly string __outPath =
            axionTestingDir.FullName + "\\Files\\out\\";

        private string outPath {
            get {
                if (!Directory.Exists(__outPath)) {
                    Directory.CreateDirectory(__outPath);
                }
                return __outPath;
            }
        }

        private readonly string __inPath =
            axionTestingDir.FullName + "\\Files\\in\\";

        private string inPath {
            get {
                if (!Directory.Exists(__inPath)) {
                    Directory.CreateDirectory(__inPath);
                }
                return __inPath;
            }
        }

        private const string testExtension = ".unit";

        private readonly List<FileInfo> sourceFiles = new List<FileInfo>();

        #endregion

        /// <summary>
        ///     A quick way to clear unit tests debug output.
        /// </summary>
        [OneTimeSetUp]
        public void ClearDebugDirectory() {
            // clear debugging output
            string dbg = __outPath + "debug\\";
            if (Directory.Exists(dbg)) {
                foreach (FileInfo file in new DirectoryInfo(dbg).EnumerateFiles()) {
                    file.Delete();
                }
            }
            else {
                Directory.CreateDirectory(dbg);
            }

            // scan for sources
            var patternsDir = new DirectoryInfo(samplesPath + "design patterns\\");
            Assert.That(patternsDir.Exists);
            ScanSources(patternsDir);
        }

        private void ScanSources(DirectoryInfo dir) {
            foreach (FileInfo file in dir.EnumerateFiles()) {
                if (file.Extension == Spec.SourceFileExtension) {
                    sourceFiles.Add(file);
                }
            }
            foreach (DirectoryInfo childDir in dir.GetDirectories()) {
                ScanSources(childDir);
            }
        }

        private SourceCode MakeSourceFromFile(string fileName) {
            return new SourceCode(
                new FileInfo(inPath + fileName + Spec.SourceFileExtension),
                outPath + fileName + testExtension
            );
        }

        [Test]
        public void NestedMultilineCommentInvalid() {
            string[] files = Directory.GetFiles(
                inPath,
                $"{nameof(NestedMultilineCommentInvalid)}*{Spec.SourceFileExtension}"
            );

            // check for error
            for (var i = 1; i < files.Length + 1; i++) {
                SourceCode source = MakeSourceFromFile(nameof(NestedMultilineCommentInvalid) + "_" + i);
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
                SourceCode source = MakeSourceFromFile(nameof(NestedMultilineCommentValid) + "_" + i);
                source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
                Assert.AreEqual(0, source.Blames.Count);
            }
        }

        [Test]
        public void StringsValidation() {
            SourceCode source = MakeSourceFromFile(nameof(StringsValidation));
            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(6, source.Blames.Count);
        }

        [Test]
        public void VariousStuffValid() {
            SourceCode source = MakeSourceFromFile(nameof(VariousStuffValid));
            source.Process(SourceProcessingMode.Lex, SourceProcessingOptions.SyntaxAnalysisDebugOutput);
            Assert.AreEqual(0, source.Blames.Count);
        }

        [Test]
        public void DesignPatternsValidation() {
            for (var i = 0; i < sourceFiles.Count; i++) {
                FileInfo file = sourceFiles[i];
                var source = new SourceCode(
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