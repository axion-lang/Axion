using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Lexer;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Parser;
using Axion.Core.Processing.Syntax.Tree;
using Axion.Core.Specification;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Container of Axion source code;
    ///     different kinds of code processing
    ///     are performed with that class.
    /// </summary>
    [JsonObject]
    public sealed class SourceUnit {
        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        [JsonProperty]
        public readonly string Code;

        /// <summary>
        ///     Tokens list generated from [<see cref="Code" />].
        /// </summary>
        [JsonProperty]
        public readonly List<Token> Tokens = new List<Token>();

        /// <summary>
        ///     Abstract Syntax Tree generated from [<see cref="Tokens" />].
        /// </summary>
        [JsonProperty]
        public readonly Ast SyntaxTree;

        /// <summary>
        ///     Contains all errors, warnings and messages
        ///     that raised on time of source processing.
        /// </summary>
        [JsonProperty]
        public readonly List<Exception> Blames = new List<Exception>();
        
        
        [JsonProperty]
        public SourceProcessingOptions Options { get; internal set; }

        #region Constructors

        /// <summary>
        ///     Creates new [<see cref="SourceUnit" />] instance
        ///     using specified [<paramref name="code" />].
        /// </summary>
        public SourceUnit(string code, string outFilePath = null) : this(
            code.Split(Spec.EndOfLines, StringSplitOptions.None),
            outFilePath
        ) {
        }

        /// <summary>
        ///     Creates new [<see cref="SourceUnit" />] instance
        ///     using specified [<paramref name="file" />] info.
        /// </summary>
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
            SyntaxTree = new Ast(this);
            Code       = File.ReadAllText(file.FullName);
            if (!Code.EndsWith("\n")) {
                Code += "\n";
            }
        }

        /// <summary>
        ///     Creates new [<see cref="SourceUnit" />] instance
        ///     using specified [<paramref name="sourceLines" />].
        ///     Use only for interpreter and tests,
        ///     output is redirected to the compiler dir.
        /// </summary>
        public SourceUnit(string[] sourceLines, string outFilePath = null) {
            InitializeFilePaths(
                Compiler.OutputDirectory + DateTime.Now.ToFileName() + Spec.SourceFileExtension,
                outFilePath
            );

            // set content
            SyntaxTree = new Ast(this);
            Code       = string.Join("\n", sourceLines) + "\n";
        }

        #endregion

        /// <summary>
        ///     Performs [<see cref="SourceUnit" />] processing
        ///     with [<see cref="mode" />] and [<see cref="options" />].
        /// </summary>
        public void Process(SourceProcessingMode mode, SourceProcessingOptions options = SourceProcessingOptions.None) {
            Options = options;
            ConsoleUI.LogInfo($"- Compiling '{SourceFileName}'");
            if (Code.Length == 0) {
                ConsoleUI.LogError("Source is empty. Compilation aborted.");
                FinishCompilation();
                return;
            }
            // [1] Lexical analysis
            LexicalAnalysis();
            if (mode == SourceProcessingMode.Lex) {
                FinishCompilation();
                return;
            }
            // [2] Parsing
            ParseAst();
            if (mode == SourceProcessingMode.Parsing) {
                FinishCompilation();
                return;
            }
            // [3] Show AST
            if (options.HasFlag(SourceProcessingOptions.ShowAstJson)) {
                string json = JsonConvert.SerializeObject(SyntaxTree, Compiler.JsonSerializer);
                ConsoleUI.WriteLine(
                    Regex.Replace(json, @"\$type.+?(\w+?),.*\""", "$type\": \"$1\"")
                );
            }
            // [4] Code generation
            GenerateCode(mode);
            FinishCompilation();
        }

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
            if (!outFile.Directory.Exists) {
                outFile.Directory.Create();
            }
            OutputFilePath = outFilePath;
        }

        private void LexicalAnalysis() {
            ConsoleUI.LogInfo("--- Tokens list generation");
            new Lexer(this).Process();
        }

        private void ParseAst() {
            ConsoleUI.LogInfo("--- Abstract Syntax Tree generation");
            new SyntaxParser(this).Process();
        }

        private void GenerateCode(SourceProcessingMode mode) {
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
                ConsoleUI.LogInfo($"--- Saving debugging information to '{DebugFilePath}'");
                SaveDebugInfoToFile();
            }

            bool hasBlames = Blames.Count > 0;
            if (hasBlames) {
                foreach (Exception blame in Blames) {
                    var exception = (LanguageException) blame;
                    exception.Print();
                }
            }

            if (hasBlames) {
                ConsoleUI.LogInfo("- Compilation aborted due to errors above.");
            }
            else {
                ConsoleUI.LogInfo("- Compilation completed.");
            }
        }

        /// <summary>
        ///     Saves processed source debug
        ///     information in JSON format.
        /// </summary>
        private void SaveDebugInfoToFile() {
            File.WriteAllText(
                DebugFilePath,
                JsonConvert.SerializeObject(this, Compiler.JsonSerializer)
            );
        }

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
        internal string OutputFilePath { get; private set; }

        /// <summary>
        ///     Path to file where source code is located.
        ///     When not specified in constructor,
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

        #endregion

        #region Errors reporting

        internal void ReportError(string message, SpannedRegion mark) {
            Blames.Add(
                new LanguageException(
                    new Blame(message, BlameSeverity.Error, mark.Span),
                    this
                )
            );
        }

        internal void Blame(BlameType type, SpannedRegion region) {
            Blame(type, region.Span.StartPosition, region.Span.EndPosition);
        }

        internal void Blame(BlameType type, Position start, Position end) {
            Debug.Assert(type != BlameType.None);

            Blames.Add(
                new LanguageException(new Blame(type, Spec.Blames[type], start, end), this)
            );
        }

        #endregion
    }
}