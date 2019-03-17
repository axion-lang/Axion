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
    }
}