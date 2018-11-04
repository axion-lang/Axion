using System;
using System.Collections.Generic;
using System.IO;
using Axion.Core.Tokens;
using Axion.Core.Tokens.Ast;
using Axion.Core.Visual;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Container of Axion source code;
    ///     performs different kinds of code processing.
    /// </summary>
    public sealed class SourceCode {
        /// <summary>
        ///     File extension of <see cref="debugFilePath" />.
        /// </summary>
        private const string debugExtension = ".debugInfo.json";

        /// <summary>
        ///     Tokens list generated from source.
        /// </summary>
        public readonly LinkedList<Token> Tokens = new LinkedList<Token>();

        /// <summary>
        ///     Abstract Syntax Tree generated from <see cref="Tokens" />.
        /// </summary>
        public readonly Ast SyntaxTree;

        /// <summary>
        ///     Contains all errors that raised while processing <see cref="SourceCode" />.
        /// </summary>
        public readonly List<SyntaxError> Errors = new List<SyntaxError>();

        /// <summary>
        ///     Contains all warnings that found while processing <see cref="SourceCode" />.
        /// </summary>
        public readonly List<SyntaxError> Warnings = new List<SyntaxError>();

        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        public readonly string Code;

        public SourceProcessingOptions Options;

        /// <summary>
        ///     Path to file where generated result is located.
        ///     If not specified in constructor, then this is assigned to
        ///     <see cref="Compiler.OutputDirectory" />\ + "latest" + <see cref="Compiler.OutputFileExtension" />
        /// </summary>
        internal readonly string OutputFilePath;

        /// <summary>
        ///     Path to file where source code is located.
        ///     When <see cref="SourceCode(string, string)" /> or
        ///     <see cref="SourceCode(string[], string)" /> is used,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        internal readonly string SourceFilePath;

        /// <summary>
        ///     Name of file where source code is located.
        ///     Created automatically from <see cref="SourceFilePath" />.
        /// </summary>
        private readonly string sourceFileName;

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        private readonly string debugFilePath;

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="file" />&gt; info.
        /// </summary>
        public SourceCode(FileInfo file, string outFilePath = null) {
            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }
            if (file.Extension != Compiler.SourceFileExtension) {
                throw new ArgumentException("Source file must have '" + Compiler.SourceFileExtension + "' extension.");
            }
            // initialize file paths
            SourceFilePath = file.Name;
            sourceFileName = Path.GetFileName(SourceFilePath);
            OutputFilePath = BuildOutputPath(outFilePath);
            debugFilePath = Compiler.DebugDirectory +
                            Path.GetFileNameWithoutExtension(OutputFilePath) +
                            debugExtension;

            // initialize content
            SyntaxTree = new Ast(this);
            Code       = File.ReadAllText(file.FullName);
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="code" />&gt;.
        /// </summary>
        public SourceCode(string code, string outFilePath = null) :
            this(
                code.Split(
                    Spec.Newlines,
                    StringSplitOptions.None
                ), outFilePath
            ) {
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="sourceLines" />&gt;.
        /// </summary>
        public SourceCode(string[] sourceLines, string outFilePath = null) {
            // initialize file paths
            SourceFilePath = Compiler.OutputDirectory + DateTime.Now.ToFileName() + Compiler.SourceFileExtension;
            sourceFileName = Path.GetFileName(SourceFilePath);
            OutputFilePath = BuildOutputPath(outFilePath);
            debugFilePath = Compiler.DebugDirectory +
                            Path.GetFileNameWithoutExtension(OutputFilePath) +
                            debugExtension;

            // initialize content
            SyntaxTree = new Ast(this);
            Code       = string.Join(Spec.EndOfLine.ToString(), sourceLines);
        }

        /// <summary>
        ///     Performs <see cref="SourceCode" /> processing
        ///     due to <see cref="mode" /> and <see cref="options" />.
        /// </summary>
        public void Process(SourceProcessingMode mode, SourceProcessingOptions options = SourceProcessingOptions.None) {
            Options = options;
            ConsoleUI.LogInfo($"--- Compiling '{sourceFileName}'");
            if (Code.Length == 0) {
                ConsoleUI.LogError("Source is empty. Compilation aborted.");
                goto COMPILATION_END;
            }
            // [1] Tokenizing
            ConsoleUI.LogInfo("-- Tokens list generation");
            {
                new Lexer(Code, Tokens, Errors, Warnings, Options).Process();
                if (mode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            // [2] Parsing
            ConsoleUI.LogInfo("-- Abstract Syntax Tree generation");
            {
                // new Parser(this).Process();
                if (mode == SourceProcessingMode.Parsing) {
                    goto COMPILATION_END;
                }
            }
            // [3] Code generation
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

            COMPILATION_END:

            if (Options.HasFlag(SourceProcessingOptions.SyntaxAnalysisDebugOutput)) {
                ConsoleUI.LogInfo($"-- Saving debugging information to '{debugFilePath}'");
                SaveDebugInfoToFile(debugFilePath, Tokens, SyntaxTree);
            }

            bool hasErrors = Errors.Count > 0;
            if (hasErrors) {
                foreach (SyntaxError error in Errors) {
                    error.Draw();
                }
            }
            if (Warnings.Count > 0) {
                foreach (SyntaxError warning in Warnings) {
                    warning.Draw();
                }
            }

            if (hasErrors) {
                ConsoleUI.LogInfo("--- Compilation aborted due to errors above.");
            }
            else {
                ConsoleUI.LogInfo("--- Compilation completed.");
            }
        }

        private string BuildOutputPath(string outFilePath) {
            if (string.IsNullOrWhiteSpace(outFilePath)) {
                outFilePath = Path.GetFileNameWithoutExtension(SourceFilePath) + Compiler.OutputFileExtension;
            }
            if (!Path.HasExtension(outFilePath)) {
                outFilePath += Compiler.OutputFileExtension;
            }
            if (!Path.IsPathRooted(outFilePath)) {
                outFilePath = Compiler.OutputDirectory + outFilePath;
            }
            return outFilePath;
        }

        /// <summary>
        ///     Saves <see cref="SourceCode" /> debug information
        ///     in JSON format.
        /// </summary>
        private static void SaveDebugInfoToFile(string debugPath, LinkedList<Token> tokens, Ast syntaxTree) {
            string debugInfo =
                "{" +
                Environment.NewLine +
                "\"tokens\": " +
                JsonConvert.SerializeObject(tokens, Compiler.JsonSerializer) +
                "," +
                Environment.NewLine +
                "\"syntaxTree\": " +
                JsonConvert.SerializeObject(syntaxTree, Compiler.JsonSerializer) +
                Environment.NewLine +
                "}";
            File.WriteAllText(debugPath, debugInfo);
        }
    }
}