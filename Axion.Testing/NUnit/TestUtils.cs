using System;
using System.IO;
using Axion.Core;
using Axion.Core.Hierarchy;
using Axion.Core.Specification;

namespace Axion.Testing.NUnit {
    public static class TestUtils {
        #region Test source files locations

        private static readonly DirectoryInfo axionTestingDir =
            new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!
                .Parent!;

        private static readonly string samplesPath = Path.Join(
            axionTestingDir.Parent!.FullName,
            "misc",
            "code-examples"
        );

        public static string SamplesPath {
            get {
                if (!Directory.Exists(samplesPath)) {
                    Directory.CreateDirectory(samplesPath);
                }

                return samplesPath;
            }
        }

        private static readonly string outPath = Path.Join(
            axionTestingDir.FullName,
            "test-files",
            "out"
        );

        public static string OutPath {
            get {
                if (!Directory.Exists(outPath)) {
                    Directory.CreateDirectory(outPath);
                }

                return outPath;
            }
        }

        private static readonly string inPath = Path.Join(
            axionTestingDir.FullName,
            "test-files"
        );

        public static string InPath {
            get {
                if (!Directory.Exists(inPath)) {
                    Directory.CreateDirectory(inPath);
                }

                return inPath;
            }
        }

        public static string StdLibPath {
            get {
                var path = Path.Join(inPath, "stdlib");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        #endregion

        internal static FileInfo FileFromTestName(string fileName) {
            return new FileInfo(
                Path.Combine(InPath, fileName + Spec.FileExtension)
            );
        }

        internal static Unit UnitFromFile(string fileName) {
            return Unit.FromFile(FileFromTestName(fileName));
        }

        internal static Unit UnitFromCode(string code) {
            return Unit.FromCode(code);
        }

        internal static (Unit, Module) ModuleFromCode(string code) {
            var unit = Unit.FromCode(code);
            var root = Module.RawFrom(unit.SourceFile.Directory!);
            root.Bind(unit);
            Compiler.Process(root, new ProcessingOptions(Mode.Parsing));
            return (unit, root);
        }

        internal static readonly FileInfo StdLibMacrosFile =
            new FileInfo(Path.Join(StdLibPath, "macros.ax"));
    }
}
