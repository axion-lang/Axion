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
        ///     Tokens list generated from source.
        /// </summary>
        public readonly LinkedList<Token> Tokens = new LinkedList<Token>();

        /// <summary>
        ///     Abstract Syntax Tree generated from <see cref="Tokens" />.
        /// </summary>
        public readonly LinkedList<Token> SyntaxTree = new LinkedList<Token>();

        /// <summary>
        ///     Contains all errors that happened while processing <see cref="SourceCode"/>.
        /// </summary>
        public readonly List<ProcessingException> Errors = new List<ProcessingException>();

        /// <summary>
        ///     Source code picked from string or file.
        /// </summary>
        public readonly string[] Content;

        /// <summary>
        ///     Path to file with source code.
        /// </summary>
        private readonly string sourceFileName;

        /// <summary>
        ///     File extension of <see cref="debugFilePath" />.
        /// </summary>
        private const string debugExtension = "_debugInfo.json";

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
                                ? ""
                                : Compiler.WorkDirectory + "output\\";
            debugFilePath += sourceFileName + debugExtension;
            Content = sourceCode.Split(
                Spec.Newlines,
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="file" />&gt; info.
        /// </summary>
        public SourceCode(FileInfo file) {
            if (!file.Exists) {
                throw new FileNotFoundException("Source file doesn't exists", file.FullName);
            }
            if (file.Extension != ".ax") {
                throw new ArgumentException("Source file must have \".ax\" extension");
            }
            sourceFileName = file.Name;
            debugFilePath  = file.FullName + debugExtension;
            Content = File.ReadAllText(file.FullName).Split(
                Spec.Newlines,
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        /// <summary>
        ///     Performs <see cref="SourceCode"/> processing
        ///     due to <see cref="processingMode"/>.
        /// </summary>
        internal void Process(SourceProcessingMode processingMode) {
            Logger.Info($"## Compiling '{sourceFileName}' ...");
            if (Content.Length == 0) {
                Logger.Error("# Source is empty. Lexical analysis aborted.");
                goto COMPILATION_END;
            }
            Logger.Info("# Tokens list generation...");
            {
                CorrectFormat();
                Lexer.Tokenize(this);
                if (processingMode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            Logger.Info("# Abstract Syntax Tree generation...");
            {
                // Program.Parser.Process(Tokens, SyntaxTree);
                if (processingMode == SourceProcessingMode.Parsing) {
                    goto COMPILATION_END;
                }
            }
            switch (processingMode) {
                case SourceProcessingMode.Interpret: {
                    Logger.Error("Interpretation support is in progress!");
                    break;
                }
                case SourceProcessingMode.ConvertC: {
                    Logger.Error("Transpiling to 'C' is not implemented yet.");
                    break;
                }
                default: {
                    Logger.Error($"'{processingMode:G}' mode not implemented yet.");
                    break;
                }
            }

            COMPILATION_END:

            // TODO show all exceptions
            if (Errors.Count > 0) {
                throw Errors[0];
            }

            if (Compiler.Options.Debug) {
                Logger.Info($"# Saving debugging information to '{debugFilePath}' ...");
                SaveDebugInfoToFile();
            }
            Logger.Info($"Compilation of \"{sourceFileName}\" completed.");
            Console.WriteLine();
        }

        /// <summary>
        ///     Saves <see cref="SourceCode"/> debug information
        ///     in JSON format.
        /// </summary>
        public void SaveDebugInfoToFile() {
            var debugInfo =
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
        ///     Appends newline statements on each line and
        ///     adds <see cref="Spec.EndFile" /> mark at last line end.
        /// </summary>
        private void CorrectFormat() {
            // append newline statements
            for (var i = 0; i < Content.Length - 1; i++) {
                Content[i] += Spec.EndLine;
            }

            // append end of file mark to last source line.
            Content[Content.Length - 1] += Spec.EndFile;
        }
    }
}