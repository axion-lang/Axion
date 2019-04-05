using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class Tests {
        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent;

        private readonly string samplesPath =
            axionTestingDir.Parent.FullName + "\\Other\\Code Examples\\";

        protected string SamplesPath {
            get {
                if (!Directory.Exists(samplesPath)) {
                    Directory.CreateDirectory(samplesPath);
                }

                return samplesPath;
            }
        }

        private readonly string outPath = axionTestingDir.FullName + "\\Files\\out\\";

        protected string OutPath {
            get {
                if (!Directory.Exists(outPath)) {
                    Directory.CreateDirectory(outPath);
                }

                return outPath;
            }
        }

        private readonly string inPath = axionTestingDir.FullName + "\\Files\\in\\";

        protected string InPath {
            get {
                if (!Directory.Exists(inPath)) {
                    Directory.CreateDirectory(inPath);
                }

                return inPath;
            }
        }

        protected const    string         TestExtension = ".unit";
        protected readonly List<FileInfo> SourceFiles   = new List<FileInfo>();

        /// <summary>
        ///     A quick way to clear unit tests debug output.
        /// </summary>
        [OneTimeSetUp]
        public void ClearDebugDirectory() {
            // clear debugging output
            string dbg = outPath + "debug\\";
            if (Directory.Exists(dbg)) {
                foreach (FileInfo file in new DirectoryInfo(dbg).EnumerateFiles()) {
                    file.Delete();
                }
            }
            else {
                Directory.CreateDirectory(dbg);
            }

            // scan for sources
            var patternsDir = new DirectoryInfo(SamplesPath + "design patterns\\");
            Assert.That(patternsDir.Exists);
            ScanSources(patternsDir);
        }

        private void ScanSources(DirectoryInfo dir) {
            foreach (FileInfo file in dir.EnumerateFiles()) {
                if (file.Extension == Spec.SourceFileExtension) {
                    SourceFiles.Add(file);
                }
            }

            foreach (DirectoryInfo childDir in dir.GetDirectories()) {
                ScanSources(childDir);
            }
        }

        internal SourceUnit MakeSourceFromFile(string fileName) {
            return new SourceUnit(
                new FileInfo(InPath + fileName + Spec.SourceFileExtension),
                OutPath + fileName + TestExtension
            );
        }

        internal SourceUnit MakeSourceFromCode(string fileName, string code) {
            return new SourceUnit(code, OutPath + fileName + TestExtension);
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
                    .Where(name => name.ToLower().StartsWith("keyword"));

            foreach (string kw in definedKws) {
                Enum.TryParse(kw, out TokenType type);
                Assert.That(
                    Spec.Keywords.ContainsValue(type),
                    "Keyword '" + kw + "' is not defined in specification."
                );
            }

//            // check operators completeness
//            IEnumerable<string> definedOps =
//                Enum.GetNames(typeof(TokenType))
//                    .Where(
//                        name => name.ToLower().StartsWith("op")
//                                && !name.ToLower().StartsWith("open")
//                    );
//
//            foreach (string op in definedOps) {
//                Enum.TryParse(op, out TokenType type);
//                Assert.That(
//                    Spec.Operators.Values.Any(props => props.Type == type)
//                    || type == TokenType.NotIn
//                    || type == TokenType.IsNot,
//                    "Operator '" + op + "' is not defined in specification."
//                );
//            }

            Debug.Assert(Spec.Operators.Count == Spec.OperatorTypes.Count);

            // check blames completeness
            IEnumerable<string> definedBls =
                Enum.GetNames(typeof(BlameType))
                    .Where(name => name != nameof(BlameType.None));

            foreach (string bl in definedBls) {
                Enum.TryParse(bl, out BlameType type);
                Assert.That(
                    Spec.Blames.ContainsKey(type),
                    "Blame '" + bl + "' is not defined in specification."
                );
            }
        }
    }
}