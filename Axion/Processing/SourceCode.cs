using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Tokens;
using Axion.Tokens.Ast;
using Newtonsoft.Json;

namespace Axion.Processing {
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
        ///     Contains all errors that happened while processing <see cref="SourceCode" />.
        /// </summary>
        public readonly List<ProcessingException> Errors = new List<ProcessingException>();

        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        public readonly string[] Lines;

        /// <summary>
        ///     Path to file with source code.
        /// </summary>
        private readonly string sourceFileName;

        /// <summary>
        ///     Path to file with processing debug output.
        /// </summary>
        private readonly string debugFilePath;

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
                JsonConvert.SerializeObject(Tokens, Compiler.Options.JsonSerializer) +
                "," + Environment.NewLine +
                "\"syntaxTree\": " +
                JsonConvert.SerializeObject(SyntaxTree, Compiler.Options.JsonSerializer) +
                Environment.NewLine + "}";
            File.WriteAllText(debugFilePath, debugInfo);
        }

        /// <summary>
        ///     Performs <see cref="SourceCode" /> processing
        ///     due to <see cref="processingMode" />.
        /// </summary>
        internal void Process(SourceProcessingMode processingMode) {
            Log.Info($"## Compiling '{sourceFileName}' ...");
            if (Lines.Length == 0) {
                Log.Error("# Source is empty. Lexical analysis aborted.");
                goto COMPILATION_END;
            }
            Log.Info("# Tokens list generation...");
            {
                CorrectFormat();
                new Lexer(this).Process();
                if (processingMode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            Log.Info("# Abstract Syntax Tree generation...");
            {
                // new Parser(this).Process();
                if (processingMode == SourceProcessingMode.Parsing) {
                    goto COMPILATION_END;
                }
            }
            switch (processingMode) {
                case SourceProcessingMode.Interpret: {
                    Log.Error("Interpretation support is in progress!");
                    break;
                }
                case SourceProcessingMode.ConvertC: {
                    Log.Error("Transpiling to 'C' is not implemented yet.");
                    break;
                }
                default: {
                    Log.Error($"'{processingMode:G}' mode not implemented yet.");
                    break;
                }
            }

            COMPILATION_END:

            bool hasErrors = Errors.Count > 0;
            if (hasErrors) {
                for (int i = 0; i < Errors.Count; i++) {
                    Errors[i].Render();
                }
            }

            if (Compiler.Options.Debug) {
                Log.Info($"# Saving debugging information to '{debugFilePath}' ...");
                SaveDebugInfoToFile();
            }
            if (hasErrors) {
                Log.WriteLine("# Compilation aborted due to errors above.", ConsoleColor.Red);
            }
            else {
                Log.Info("# Compilation completed.");
            }
        }

        /// <summary>
        ///     Appends newline statements on each line and
        ///     adds <see cref="Spec.EndStream" /> mark at last line end.
        /// </summary>
        private void CorrectFormat() {
            // append newline statements
            for (var i = 0; i < Lines.Length - 1; i++) {
                Lines[i] += Spec.EndLine;
            }

            // append end of file mark to last source line.
            Lines[Lines.Length - 1] += Spec.EndStream;
        }
    }
}