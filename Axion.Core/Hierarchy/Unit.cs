using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Hierarchy {
    /// <summary>
    ///     Container of Axion source code;
    ///     different kinds of code processing
    ///     are performed with that class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Unit {
        #region File paths

        /// <summary>
        ///     Path to file where source code is located.
        ///     If not specified in constructor, file name
        ///     assigned to date and time of instance creation.
        /// </summary>
        public FileInfo SourceFile { get; }

        private DirectoryInfo? outputDirectory;

        /// <summary>
        ///     Path to directory where generated result is located.
        ///     Defaults to [source-directory]/../out.
        /// </summary>
        public DirectoryInfo OutputDirectory {
            get {
                if (outputDirectory != null) {
                    return outputDirectory;
                }

                if (Module == null)
                    outputDirectory = new DirectoryInfo(
                        Path.Join(SourceFile.Directory?.FullName!, "out")
                    );
                else {
                    outputDirectory = new DirectoryInfo(
                        Path.Join(
                            Module.OutputDirectory.FullName!,
                            Path.GetRelativePath(
                                Module.Directory.FullName,
                                SourceFile.DirectoryName
                            )
                        )
                    );
                }

                Utilities.ResolvePath(outputDirectory.FullName);
                return outputDirectory;
            }
        }

        #endregion

        public string Name;

        public Module? Module { get; set; }

        public Dictionary<string, Unit> Imports { get; } = new();

        public TextStream TextStream { get; private set; }

        public TokenStream TokenStream { get; }

        [JsonProperty]
        public Ast Ast { get; }

        public List<LanguageReport> Blames { get; } = new();

        public bool HasErrors => Blames.Any(b => b.Severity == BlameSeverity.Error);

        private Unit(
            string         code      = "",
            FileInfo?      source    = null,
            DirectoryInfo? outputDir = null
        ) {
            if (source == null) {
                source = new FileInfo(Compiler.GetTempSourceFilePath());
                // ReSharper disable once PossibleNullReferenceException
                Utilities.ResolvePath(source.Directory.FullName);
            }

            SourceFile = source;
            Name       = NameFromFile(SourceFile)!;

            if (outputDir != null) {
                outputDirectory = outputDir;
            }

            TextStream  = new TextStream(code);
            TokenStream = new TokenStream();
            Ast         = new Ast(this);
        }

        public static Unit FromCode(
            string         code,
            DirectoryInfo? outputDir = null
        ) {
            return new(code, outputDir: outputDir);
        }

        public static Unit FromLines(
            IEnumerable<string> lines,
            DirectoryInfo?      outputDir = null
        ) {
            return new(string.Join("\n", lines), outputDir: outputDir);
        }

        public static Unit FromFile(
            FileInfo       sourceFile,
            DirectoryInfo? outputDir = null
        ) {
            if (!sourceFile.Exists) {
                throw new FileNotFoundException(
                    $"'{sourceFile.Name}' does not exists."
                );
            }

            if (sourceFile.Extension != Spec.FileExtension) {
                throw new ArgumentException(
                    $"'{sourceFile.Name}' file must have {Spec.FileExtension} extension."
                );
            }

            return new Unit(
                File.ReadAllText(sourceFile.FullName),
                sourceFile,
                outputDir
            );
        }

        public static Unit FromInterpolation(Unit unit) {
            return new() {
                TextStream = unit.TextStream,
                Module     = unit.Module
            };
        }

        public static string? NameFromFile(FileInfo file) {
            var name = Path.GetFileNameWithoutExtension(file.FullName);
            if (name == null || name.Any(c => !c.IsValidIdPart())) {
                return null;
            }

            return name;
        }
    }
}
