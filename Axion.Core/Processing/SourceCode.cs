using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        ///     Tokens list generated from source.
        /// </summary>
        public LinkedList<Token> Tokens;

        /// <summary>
        ///     Abstract Syntax Tree generated from <see cref="Tokens" />.
        /// </summary>
        public readonly Ast SyntaxTree;

        /// <summary>
        ///     Contains all errors that happened while processing <see cref="SourceCode" />.
        /// </summary>
        public List<ProcessingException> Errors;

        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        public readonly string[] Lines;

        public SourceProcessingOptions Options = SourceProcessingOptions.None;

        /// <summary>
        ///     Path to file with source code.
        /// </summary>
        private readonly string sourceFileName;

        /// <summary>
        ///     Path to file with processing debug output.
        /// </summary>
        private readonly string debugFilePath;

        /// <summary>
        ///     File extension of <see cref="debugFilePath" />.
        /// </summary>
        private const string debugExtension = ".debugInfo.json";

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="sourceCode" />&gt;.
        /// </summary>
        public SourceCode(string sourceCode, string filePath = null) :
            this(
                sourceCode.Split(
                    Spec.Newlines,
                    StringSplitOptions.None
                ), filePath
            ) {
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="sourceLines" />&gt;.
        /// </summary>
        public SourceCode(IEnumerable<string> sourceLines, string filePath = null) {
            SyntaxTree = new Ast(this);

            sourceFileName = filePath ?? "latest.unittest.ax";
            debugFilePath = Path.IsPathRooted(sourceFileName)
                                ? sourceFileName + debugExtension
                                : Compiler.DebugDirectory + sourceFileName + debugExtension;
            Lines = sourceLines.ToArray();
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="file" />&gt; info.
        /// </summary>
        public SourceCode(FileInfo file, string outFilePath = null) {
            SyntaxTree = new Ast(this);

            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }
            if (file.Extension != ".ax") {
                throw new ArgumentException("Source file must have \".ax\" extension");
            }
            sourceFileName = file.Name;
            debugFilePath = outFilePath != null
                                ? Path.IsPathRooted(outFilePath)
                                      ? outFilePath + debugExtension
                                      : Compiler.DebugDirectory + outFilePath + debugExtension
                                : file.FullName + debugExtension;
            Lines = File.ReadAllText(file.FullName).Split(
                Spec.Newlines,
                StringSplitOptions.None
            );
        }

        /// <summary>
        ///     Saves <see cref="SourceCode" /> debug information
        ///     in JSON format.
        /// </summary>
        private void SaveDebugInfoToFile() {
            string debugInfo =
                "{" + Environment.NewLine +
                "\"tokens\": " +
                JsonConvert.SerializeObject(Tokens, Compiler.JsonSerializer) +
                "," + Environment.NewLine +
                "\"syntaxTree\": " +
                JsonConvert.SerializeObject(SyntaxTree, Compiler.JsonSerializer) +
                Environment.NewLine + "}";
            File.WriteAllText(debugFilePath, debugInfo);
        }

        /// <summary>
        ///     Performs <see cref="SourceCode" /> processing
        ///     due to <see cref="mode" /> and <see cref="options" />.
        /// </summary>
        internal void Process(SourceProcessingMode mode, SourceProcessingOptions options = SourceProcessingOptions.None) {
            Options = options;
            ConsoleLog.Info($"## Compiling '{sourceFileName}' ...");
            if (Lines.Length == 0) {
                ConsoleLog.Error("# Source is empty. Lexical analysis aborted.");
                goto COMPILATION_END;
            }
            ConsoleLog.Info("# Tokens list generation...");
            {
                CorrectFormat();
                new Lexer(Lines, out Tokens, out Errors, Options).Process();
                if (mode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            ConsoleLog.Info("# Abstract Syntax Tree generation...");
            {
                // new Parser(this).Process();
                if (mode == SourceProcessingMode.Parsing) {
                    goto COMPILATION_END;
                }
            }
            switch (mode) {
                case SourceProcessingMode.Interpret: {
                    ConsoleLog.Error("Interpretation support is in progress!");
                    break;
                }
                case SourceProcessingMode.ConvertC: {
                    ConsoleLog.Error("Transpiling to 'C' is not implemented yet.");
                    break;
                }
                default: {
                    ConsoleLog.Error($"'{mode:G}' mode not implemented yet.");
                    break;
                }
            }

            COMPILATION_END:

            if (Options.HasFlag(SourceProcessingOptions.SyntaxAnalysisDebugOutput)) {
                ConsoleLog.Info($"# Saving debugging information to '{debugFilePath}' ...");
                SaveDebugInfoToFile();
            }

            if (Errors.Count > 0) {
                for (var i = 0; i < Errors.Count; i++) {
                    Errors[i].Render();
                }
                ConsoleLog.Info("# Compilation aborted due to errors above.");
            }
            else {
                ConsoleLog.Info("# Compilation completed.");
            }
        }

        /// <summary>
        ///     Appends newline statements on each line and
        ///     adds <see cref="Spec.EndStream" /> mark at last line end.
        /// </summary>
        private void CorrectFormat() {
            // append newline statements to all lines except last
            for (var i = 0; i < Lines.Length - 1; i++) {
                Lines[i] += Spec.EndLine;
            }
        }
    }
}