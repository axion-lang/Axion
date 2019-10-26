using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
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
        public ProcessingOptions Options        = ProcessingOptions.None;
        public ProcessingMode    ProcessingMode = ProcessingMode.None;

        public List<LangException> Blames { get; } = new List<LangException>();

        public TextStream TextStream { get; private set; }

        [JsonProperty]
        public TokenStream TokenStream { get; }

        public Stack<Token> MismatchingPairs { get; } = new Stack<Token>();

        public List<TokenType> ProcessTerminators { get; } = new List<TokenType> {
            TokenType.End
        };

        [JsonProperty]
        public Ast Ast { get; set; }

        public char IndentChar = '\0';
        public int  IndentSize;
        public int  LastIndentLen;
        public int  IndentLevel;

        #region File paths

        /// <summary>
        ///     Path to file where source code is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public FileInfo SourceFilePath { get; }

        /// <summary>
        ///     Path to file where generated result is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public FileInfo OutputFilePath { get; }

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        public FileInfo DebugFilePath { get; }

        #endregion

        private SourceUnit(
            string   code       = "",
            FileInfo sourcePath = null,
            FileInfo outputPath = null,
            FileInfo debugPath  = null
        ) {
            if (sourcePath == null) {
                sourcePath = Compiler.CreateTempSourcePath();
                Utilities.ResolvePath(sourcePath.Directory?.FullName);
            }

            SourceFilePath = sourcePath;
            if (SourceFilePath.Extension != Compiler.SourceFileExt) {
                throw new ArgumentException(
                    $"Source file must have {Compiler.SourceFileExt} extension.",
                    nameof(sourcePath)
                );
            }

            if (outputPath == null) {
                outputPath = new FileInfo(
                    Path.Combine(
                        SourceFilePath.Directory.FullName,
                        "out",
                        SourceFilePath.Name + Compiler.OutputFileExt
                    )
                );
            }

            OutputFilePath = outputPath;

            if (debugPath == null) {
                debugPath = new FileInfo(
                    Path.Combine(
                        SourceFilePath.Directory.FullName,
                        "debug",
                        SourceFilePath.Name + Compiler.DebugFileExt
                    )
                );
            }

            DebugFilePath = debugPath;

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