using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Source {
    /// <summary>
    ///     Container of Axion source code;
    ///     different kinds of code processing
    ///     are performed with that class.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SourceUnit {
        public const string SourceFileExt = ".ax";
        public const string DebugFileExt  = ".dbg.json";

        public string OutputFileExt {
            get {
                if (Options.HasFlag(ProcessingOptions.ToAxion)) {
                    return ".ax";
                }

                if (Options.HasFlag(ProcessingOptions.ToCSharp)) {
                    return ".cs";
                }

                if (Options.HasFlag(ProcessingOptions.ToPython)) {
                    return ".py";
                }

                if (Options.HasFlag(ProcessingOptions.ToPascal)) {
                    return ".pas";
                }

                return ".out";
            }
        }

        internal ProcessingOptions   Options        = ProcessingOptions.None;
        internal ProcessingMode      ProcessingMode = ProcessingMode.None;
        public   List<LangException> Blames      { get; } = new List<LangException>();
        public   TextStream          TextStream  { get; private set; }
        public   TokenStream         TokenStream { get; }

        [JsonProperty]
        public Ast Ast { get; set; }

        public CodeWriter CodeWriter { get; set; }

        #region File paths

        /// <summary>
        ///     Path to file where source code is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public FileInfo SourceFilePath { get; }

        private FileInfo outputFilePath;

        /// <summary>
        ///     Path to file where generated result is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public FileInfo OutputFilePath {
            get {
                FileInfo path = outputFilePath;
                if (path != null) {
                    return path;
                }

                var f = new FileInfo(
                    Path.Combine(
                        SourceFilePath.Directory.FullName,
                        "out",
                        Path.GetFileNameWithoutExtension(SourceFilePath.FullName) + OutputFileExt
                    )
                );
                Utilities.ResolvePath(f.Directory.FullName);
                return f;
            }
        }

        private FileInfo debugFilePath;

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        public FileInfo DebugFilePath {
            get {
                FileInfo path = debugFilePath;
                if (path != null) {
                    return path;
                }

                var f = new FileInfo(
                    Path.Combine(
                        SourceFilePath.Directory.FullName,
                        "debug",
                        Path.GetFileNameWithoutExtension(SourceFilePath.FullName) + DebugFileExt
                    )
                );
                Utilities.ResolvePath(f.Directory.FullName);
                return f;
            }
        }

        #endregion

        private SourceUnit(
            string   code       = "",
            FileInfo sourcePath = null,
            FileInfo outputPath = null,
            FileInfo debugPath  = null
        ) {
            if (sourcePath == null) {
                sourcePath = new FileInfo(
                    Path.Combine(
                        Compiler.WorkDir,
                        "temp",
                        "tmp-" + DateTime.Now.ToFileName() + SourceFileExt
                    )
                );
                Utilities.ResolvePath(sourcePath.Directory?.FullName);
            }

            SourceFilePath = sourcePath;
            if (SourceFilePath.Extension != SourceFileExt) {
                throw new ArgumentException(
                    $"Source file must have {SourceFileExt} extension.",
                    nameof(sourcePath)
                );
            }

            if (outputPath != null) {
                outputFilePath = outputPath;
            }

            if (debugPath != null) {
                debugFilePath = debugPath;
            }

            TextStream  = new TextStream(code);
            TokenStream = new TokenStream();
            Ast         = new Ast(this);
        }

        public static SourceUnit FromCode(string code, FileInfo outFilePath = null) {
            return new SourceUnit(
                code,
                outputPath: outFilePath
            );
        }

        public static SourceUnit FromLines(string[] lines, FileInfo outFilePath = null) {
            return new SourceUnit(
                string.Join("\n", lines),
                outputPath: outFilePath
            );
        }

        public static SourceUnit FromFile(FileInfo srcFilePath, FileInfo outFilePath = null) {
            if (!srcFilePath.Exists) {
                throw new FileNotFoundException();
            }

            return new SourceUnit(
                File.ReadAllText(srcFilePath.FullName),
                srcFilePath,
                outFilePath
            );
        }

        public static SourceUnit FromInterpolation(TextStream stream) {
            return new SourceUnit {
                TextStream = stream
            };
        }
    }
}