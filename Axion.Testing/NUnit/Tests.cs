using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing;
using Axion.Core.Specification;
using NUnit.Framework;

namespace Axion.Testing.NUnit {
    [TestFixture]
    public class Tests {
        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent;

        private readonly string __samplesPath = axionTestingDir.Parent.FullName + "\\Other\\Code Examples\\";

        protected string samplesPath {
            get {
                if (!Directory.Exists(__samplesPath)) {
                    Directory.CreateDirectory(__samplesPath);
                }
                return __samplesPath;
            }
        }

        protected readonly string __outPath = axionTestingDir.FullName + "\\Files\\out\\";

        protected string outPath {
            get {
                if (!Directory.Exists(__outPath)) {
                    Directory.CreateDirectory(__outPath);
                }
                return __outPath;
            }
        }

        private readonly string __inPath = axionTestingDir.FullName + "\\Files\\in\\";

        protected string inPath {
            get {
                if (!Directory.Exists(__inPath)) {
                    Directory.CreateDirectory(__inPath);
                }
                return __inPath;
            }
        }

        protected const string testExtension = ".unit";

        protected readonly List<FileInfo> sourceFiles = new List<FileInfo>();

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

        internal SourceUnit MakeSourceFromFile(string fileName) {
            return new SourceUnit(
                new FileInfo(inPath + fileName + Spec.SourceFileExtension),
                outPath + fileName + testExtension
            );
        }

        internal SourceUnit MakeSourceFromCode(string fileName, string code) {
            return new SourceUnit(code, outPath + fileName + testExtension);
        }
    }
}