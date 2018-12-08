using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Container of Axion source code;
    ///     performs different kinds of code processing.
    /// </summary>
    public sealed class SourceCode {
        /// <summary>
        ///     File extension of <see cref="DebugFilePath" />.
        /// </summary>
        private const string debugExtension = ".dbg.json";

        /// <summary>
        ///     Tokens list generated from source.
        /// </summary>
        public readonly List<Token> Tokens = new List<Token>();

        /// <summary>
        ///     Abstract Syntax Tree generated from <see cref="Tokens" />.
        /// </summary>
        public readonly Ast SyntaxTree;

        /// <summary>
        ///     Contains all errors that raised while processing <see cref="SourceCode" />.
        /// </summary>
        public readonly List<Exception> Errors = new List<Exception>();

        /// <summary>
        ///     Contains all warnings that found while processing <see cref="SourceCode" />.
        /// </summary>
        public readonly List<Exception> Warnings = new List<Exception>();

        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        public readonly string Code;

        public SourceProcessingOptions Options { get; private set; }

        /// <summary>
        ///     Path to file where generated result is located.
        ///     If not specified in constructor, then this is assigned to
        ///     <see cref="Compiler.OutputDirectory" />\ + "latest" + <see cref="Compiler.OutputFileExtension" />
        /// </summary>
        internal string OutputFilePath { get; private set; }

        /// <summary>
        ///     Path to file where source code is located.
        ///     When <see cref="SourceCode(string, string)" /> or
        ///     <see cref="SourceCode(string[], string)" /> is used,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        internal string SourceFilePath { get; private set; }

        /// <summary>
        ///     Name of file where source code is located.
        ///     Created automatically from <see cref="SourceFilePath" />.
        /// </summary>
        internal string SourceFileName { get; private set; }

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        internal string DebugFilePath { get; private set; }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="code" />&gt;.
        /// </summary>
        public SourceCode(string code, string outFilePath = null) :
            this(
                code.Split(
                    Spec.EndOfLines,
                    StringSplitOptions.None
                ), outFilePath
            ) {
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="file" />&gt; info.
        /// </summary>
        public SourceCode(FileInfo file, string outFilePath = null) {
            // check source file
            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }
            if (file.Extension != Compiler.SourceFileExtension) {
                throw new ArgumentException(
                    "Source file must have '" + Compiler.SourceFileExtension + "' extension.",
                    nameof(file)
                );
            }

            // initialize file paths
            InitializeFilePaths(file.FullName, outFilePath);

            // set content
            SyntaxTree = new Ast(this);
            Code       = File.ReadAllText(file.FullName);
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="sourceLines" />&gt;.
        ///     Use only for interpreter and tests,
        ///     output is redirected to the compiler dir.
        /// </summary>
        public SourceCode(string[] sourceLines, string outFilePath = null) {
            InitializeFilePaths(
                Compiler.OutputDirectory + DateTime.Now.ToFileName() + Compiler.SourceFileExtension,
                outFilePath
            );

            // set content
            SyntaxTree = new Ast(this);
            Code       = string.Join("\n", sourceLines);
        }

        private void InitializeFilePaths(
            string sourceFilePath,
            string outFilePath = null
        ) {
            SourceFilePath = sourceFilePath;
            SourceFileName = Path.GetFileNameWithoutExtension(SourceFilePath);
            BuildOutputPath(outFilePath);

            string debugDir = new FileInfo(OutputFilePath).Directory.FullName +
                              "\\debug\\";
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
                outFilePath += Compiler.OutputFileExtension;
            }
            if (!Path.IsPathRooted(outFilePath)) {
                outFilePath = Compiler.OutputDirectory + outFilePath;
            }
            var outFile = new FileInfo(outFilePath);
            if (!outFile.Directory.Exists) {
                outFile.Directory.Create();
            }
            OutputFilePath = outFilePath;
        }

        /// <summary>
        ///     Performs <see cref="SourceCode" /> processing
        ///     due to <see cref="mode" /> and <see cref="options" />.
        /// </summary>
        public void Process(
            SourceProcessingMode    mode,
            SourceProcessingOptions options = SourceProcessingOptions.None
        ) {
            Options = options;
            ConsoleUI.LogInfo($"--- Compiling '{SourceFileName}'");
            if (Code.Length == 0) {
                ConsoleUI.LogError("Source is empty. Compilation aborted.");
                FinishCompilation();
                return;
            }
            // [1] Tokenizing
            Tokenizing();
            if (mode == SourceProcessingMode.Lex) {
                FinishCompilation();
                return;
            }
            // [2] Parsing
            Parsing();
            if (mode == SourceProcessingMode.Parsing) {
                FinishCompilation();
                return;
            }
            // [3] Code generation
            GenerateCode(mode);
            FinishCompilation();
        }

        private void Tokenizing() {
            ConsoleUI.LogInfo("-- Tokens list generation");
            new Lexer(Code, Tokens, Errors, Warnings, Options).Process();
        }

        private void Parsing() {
            ConsoleUI.LogInfo("-- Abstract Syntax Tree generation");
        }

        private void GenerateCode(
            SourceProcessingMode mode
        ) {
            switch (mode) {
                case SourceProcessingMode.Interpret: {
                    ConsoleUI.LogError("Interpretation support is in progress!");
                    break;
                }
                case SourceProcessingMode.ConvertC: {
                    ConsoleUI.LogError("Transpiling to 'C' is not implemented yet.");
                    break;
                }
                default: {
                    ConsoleUI.LogError($"'{mode:G}' mode not implemented yet.");
                    break;
                }
            }
        }

        private void FinishCompilation() {
            if (Options.HasFlag(SourceProcessingOptions.SyntaxAnalysisDebugOutput)) {
                ConsoleUI.LogInfo($"-- Saving debugging information to '{DebugFilePath}'");
                SaveDebugInfoToFile();
            }

            bool hasErrors = Errors.Count > 0;
            if (hasErrors) {
                foreach (Exception exception in Errors) {
                    var error = (SyntaxException) exception;
                    error.Print();
                }
            }
            if (Warnings.Count > 0) {
                foreach (Exception exception in Warnings) {
                    var warning = (SyntaxException) exception;
                    warning.Print();
                }
            }

            if (hasErrors) {
                ConsoleUI.LogInfo("--- Compilation aborted due to errors above.");
            }
            else {
                ConsoleUI.LogInfo("--- Compilation completed.");
            }
        }

        /// <summary>
        ///     Saves <see cref="SourceCode" /> debug information
        ///     in JSON format.
        /// </summary>
        private void SaveDebugInfoToFile() {
            string debugInfo =
                $@"{{
""tokens"": {JsonConvert.SerializeObject(Tokens, Compiler.JsonSerializer)}
}}";
            File.WriteAllText(DebugFilePath, debugInfo);
        }
    }
}