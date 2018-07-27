using System;
using System.Collections.Generic;
using System.IO;
using Axion.Tokens;
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
        public readonly LinkedList<Token> SyntaxTree = new LinkedList<Token>();

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
        public SourceCode(string sourceCode, string fileName = null) {
            sourceFileName = fileName ?? "latest.unittest.ax";
            debugFilePath = Path.IsPathRooted(sourceFileName)
                                ? sourceFileName + debugExtension
                                : Compiler.DebugDirectory + sourceFileName + debugExtension;
            Lines = sourceCode.Split(
                Spec.Newlines,
                StringSplitOptions.None
            );
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="file" />&gt; info.
        /// </summary>
        public SourceCode(FileInfo file, string outFileName = null) {
            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }
            if (file.Extension != ".ax") {
                throw new ArgumentException("Source file must have \".ax\" extension");
            }
            sourceFileName = file.Name;
            debugFilePath = outFileName != null
                                ? Path.IsPathRooted(outFileName)
                                      ? outFileName + debugExtension
                                      : Compiler.DebugDirectory + outFileName + debugExtension
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
        public void SaveDebugInfoToFile() {
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
                new Lexer(this).Tokenize();
                if (processingMode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            Log.Info("# Abstract Syntax Tree generation...");
            {
                // Program.Parser.Process(Tokens, SyntaxTree);
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

            // TODO show all exceptions
            if (Errors.Count > 0) {
                for (int i = 0; i < Errors.Count; i++) {
                    Errors[i].Render();
                }
            }

            if (Compiler.Options.Debug) {
                Log.Info($"# Saving debugging information to '{debugFilePath}' ...");
                SaveDebugInfoToFile();
            }
            Log.Info($"Compilation of \"{sourceFileName}\" completed.");
            Console.WriteLine();
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