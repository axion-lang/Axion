using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Specification;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Container of Axion source code;
    ///     different kinds of code processing
    ///     are performed with that class.
    /// </summary>
    public sealed class SourceUnit {
        /// <summary>
        ///     Source code picked from string or file.
        /// </summary>
        public readonly string Code;

        /// <summary>
        ///     Tokens list generated from source code.
        /// </summary>
        public readonly List<Token> Tokens = new List<Token>();

        /// <summary>
        ///     Abstract Syntax Tree generated from tokens list.
        /// </summary>
        public readonly Ast Ast;

        /// <summary>
        ///     Contains all errors, warnings and messages
        ///     that raised on time of source processing.
        /// </summary>
        public readonly List<Exception> Blames = new List<Exception>();

        public SourceProcessingOptions Options;
        public SourceProcessingMode    ProcessingMode;

        #region File paths

        /// <summary>
        ///     File extension of <see cref="DebugFilePath" />.
        /// </summary>
        private const string debugExtension = ".dbg.json";

        /// <summary>
        ///     Path to file where generated result is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public string OutputFilePath { get; private set; }

        /// <summary>
        ///     Path to file where source code is located.
        ///     When not specified in constructor,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        public string SourceFilePath { get; private set; }

        /// <summary>
        ///     Name of file where source code is located.
        ///     Created automatically from <see cref="SourceFilePath" />.
        /// </summary>
        public string SourceFileName { get; private set; }

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        public string DebugFilePath { get; private set; }

        #endregion

        #region Constructors

        public SourceUnit(string code, string outFilePath = null) : this(
            code.Split(Spec.EndOfLines, StringSplitOptions.None),
            outFilePath
        ) { }

        public SourceUnit(FileInfo file, string outFilePath = null) {
            // check source file
            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }

            if (file.Extension != Spec.SourceFileExtension) {
                throw new ArgumentException(
                    "Source file must have '" + Spec.SourceFileExtension + "' extension.",
                    nameof(file)
                );
            }

            // initialize file paths
            InitializeFilePaths(file.FullName, outFilePath);

            // set content
            Ast  = new Ast(this);
            Code = File.ReadAllText(file.FullName);
            // add null-terminator
            Code += Spec.EndOfCode;
        }

        /// <summary>
        ///     Use only for interpreter and tests,
        ///     output is redirected to the compiler dir.
        /// </summary>
        public SourceUnit(string[] sourceLines, string outFilePath = null) {
            InitializeFilePaths(
                Compiler.OutputDirectory
                + "Temp_"
                + DateTime.Now.ToFileName()
                + Spec.SourceFileExtension,
                outFilePath
            );

            // set content
            Ast  = new Ast(this);
            Code = string.Join("\n", sourceLines);
            // add null-terminator
            Code += Spec.EndOfCode;
        }

        #endregion

        #region File path helpers

        private void InitializeFilePaths(string sourceFilePath, string outFilePath = null) {
            SourceFilePath = sourceFilePath;
            SourceFileName = Path.GetFileNameWithoutExtension(SourceFilePath);
            BuildOutputPath(outFilePath);

            string debugDir = new FileInfo(OutputFilePath).Directory.FullName + "\\debug\\";
            if (!Directory.Exists(debugDir)) {
                Directory.CreateDirectory(debugDir);
            }

            DebugFilePath = debugDir + SourceFileName + debugExtension;
        }

        private void BuildOutputPath(string outFilePath) {
            if (string.IsNullOrWhiteSpace(outFilePath)) {
                outFilePath = SourceFileName;
            }

            if (!Path.HasExtension(outFilePath)) {
                outFilePath += Spec.OutputFileExtension;
            }

            if (!Path.IsPathRooted(outFilePath)) {
                outFilePath = Compiler.OutputDirectory + outFilePath;
            }

            var outFile = new FileInfo(outFilePath);
            if (!outFile.Directory?.Exists ?? false) {
                outFile.Directory.Create();
            }

            OutputFilePath = outFilePath;
        }

        #endregion

        #region Blaming

        internal void ReportError(string message, SpannedRegion mark) {
            Blames.Add(new LanguageException(new Blame(message, BlameSeverity.Error, mark.Span)));
        }

        internal void Blame(BlameType type, SpannedRegion region) {
            Blame(type, region.Span.StartPosition, region.Span.EndPosition);
        }

        internal void Blame(BlameType type, Position start, Position end) {
            Blames.Add(new LanguageException(new Blame(type, Spec.Blames[type], start, end)));
        }

        #endregion
    }
}