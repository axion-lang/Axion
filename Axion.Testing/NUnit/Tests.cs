using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Source;
using Axion.Core.Specification;
using NLog;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class Tests {
        #region Test source files locations

        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent;

        private readonly string samplesPath = Path.Combine(
            axionTestingDir.Parent.FullName,
            "misc",
            "code-examples"
        );

        protected string SamplesPath {
            get {
                if (!Directory.Exists(samplesPath)) {
                    Directory.CreateDirectory(samplesPath);
                }

                return samplesPath;
            }
        }

        private static readonly string outPath = Path.Combine(
            axionTestingDir.FullName,
            "test-files",
            "out"
        );

        protected static string OutPath {
            get {
                if (!Directory.Exists(outPath)) {
                    Directory.CreateDirectory(outPath);
                }

                return outPath;
            }
        }

        private static readonly string inPath = Path.Combine(
            axionTestingDir.FullName,
            "test-files"
        );

        protected static string InPath {
            get {
                if (!Directory.Exists(inPath)) {
                    Directory.CreateDirectory(inPath);
                }

                return inPath;
            }
        }

        #endregion

        protected const    string         TestExtension = ".unit";
        protected readonly List<FileInfo> SourceFiles   = new List<FileInfo>();

        /// <summary>
        ///     A quick way to clear unit tests debug output.
        /// </summary>
        [OneTimeSetUp]
        public void ClearDebugDirectory() {
            LogManager.Configuration.Variables["consoleLogLevel"] = "Fatal";
            LogManager.Configuration.Variables["fileLogLevel"]    = "Fatal";
            LogManager.ReconfigExistingLoggers();
            // clear debugging output
            string dbg = Path.Combine(outPath, "debug");
            if (Directory.Exists(dbg)) {
                foreach (FileInfo file in new DirectoryInfo(dbg).EnumerateFiles()) {
                    file.Delete();
                }
            }
            else {
                Directory.CreateDirectory(dbg);
            }

            // scan for sources
            var patternsDir = new DirectoryInfo(Path.Combine(SamplesPath, "design patterns"));
            Assert.That(patternsDir.Exists);
            ScanSources(patternsDir);
        }

        private void ScanSources(DirectoryInfo dir) {
            foreach (FileInfo file in dir.EnumerateFiles()) {
                if (file.Extension == Unit.SourceFileExt) {
                    SourceFiles.Add(file);
                }
            }

            foreach (DirectoryInfo childDir in dir.GetDirectories()) {
                ScanSources(childDir);
            }
        }

        internal static Unit MakeSourceFromFile(string fileName) {
            return Unit.FromFile(
                new FileInfo(Path.Combine(InPath, fileName + Unit.SourceFileExt))
            );
        }

        internal static Unit MakeSourceFromCode(
            string                    code,
            [CallerMemberName] string fileName = null
        ) {
            return Unit.FromCode(
                code,
                new FileInfo(Path.Combine(OutPath, fileName + TestExtension))
            );
        }

        /// <summary>
        ///     First barrier preventing silly
        ///     errors in specification.
        ///     Checks, that all keywords, operators and blames
        ///     are declared in specification.
        /// </summary>
        [Test]
        public static void SpecificationCheck() {
            // check keywords completeness
            IEnumerable<string> definedKws = Enum
                                             .GetNames(typeof(TokenType))
                                             .Where(name => name.ToUpper().StartsWith("KEYWORD"));

            foreach (string kw in definedKws) {
                Enum.TryParse(kw, out TokenType type);
                Assert.That(
                    Spec.Keywords.ContainsValue(type),
                    "Keyword '" + kw + "' is not defined in specification."
                );
            }
        }
    }
}
