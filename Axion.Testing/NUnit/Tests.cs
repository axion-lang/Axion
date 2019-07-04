using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Axion.Core;
using Axion.Core.Processing;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Source;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class Tests {
        #region Test source files locations

        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent;

        private readonly string samplesPath = Path.Combine(
            axionTestingDir.Parent.FullName,
            "Other",
            "Code Examples"
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
            "Files",
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
            "Files",
            "in"
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
                if (file.Extension == Compiler.SourceFileExtension) {
                    SourceFiles.Add(file);
                }
            }

            foreach (DirectoryInfo childDir in dir.GetDirectories()) {
                ScanSources(childDir);
            }
        }

        internal static SourceUnit MakeSourceFromFile([CallerMemberName] string fileName = null) {
            return new SourceUnit(
                new FileInfo(Path.Combine(InPath, fileName + Compiler.SourceFileExtension)),
                OutPath + fileName + TestExtension
            );
        }

        internal static SourceUnit MakeSourceFromCode(
            string                    code,
            [CallerMemberName] string fileName = null
        ) {
            return new SourceUnit(code, Path.Combine(OutPath, fileName + TestExtension));
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
            IEnumerable<string> definedKws =
                Enum.GetNames(typeof(TokenType))
                    .Where(name => name.ToUpper().StartsWith("KEYWORD"));

            foreach (string kw in definedKws) {
                Enum.TryParse(kw, out TokenType type);
                Assert.That(
                    Spec.Keywords.ContainsValue(type),
                    "Keyword '" + kw + "' is not defined in specification."
                );
            }

            // check blames completeness
            foreach (string blame in Enum.GetNames(typeof(BlameType))) {
                Enum.TryParse(blame, out BlameType type);
                Assert.That(
                    Spec.Blames.ContainsKey(type),
                    "Blame '" + blame + "' is not defined in specification."
                );
            }
        }
    }
}