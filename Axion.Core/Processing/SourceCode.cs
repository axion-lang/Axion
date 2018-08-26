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
        public List<SourceProcessingException> Errors;

        /// <summary>
        ///     Lines of source code picked from string or file.
        /// </summary>
        public readonly string[] Lines;

        public SourceProcessingOptions Options = SourceProcessingOptions.None;

        /// <summary>
        ///     Path to file where source code is located.
        ///     When <see cref="SourceCode(string, string)"/> or
        ///     <see cref="SourceCode(string[], string)"/> is used,
        ///     then file name assigned to date and time of instance creation.
        /// </summary>
        private readonly string sourceFilePath;

        /// <summary>
        ///     Name of file where source code is located.
        ///     Created automatically from <see cref="sourceFilePath"/>.
        /// </summary>
        private readonly string sourceFileName;

        /// <summary>
        ///     Path to file where generated result is located.
        ///     If not specified in constructor, then this is assigned to
        ///     <see cref="Compiler.OutputDirectory"/>\ + "latest" + <see cref="Compiler.OutputFileExtension"/>
        /// </summary>
        private readonly string outputFilePath;

        /// <summary>
        ///     Path to file where processing debug output is located.
        /// </summary>
        private readonly string debugFilePath;

        /// <summary>
        ///     File extension of <see cref="debugFilePath" />.
        /// </summary>
        private const string debugExtension = ".debugInfo.json";

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
            sourceFilePath = file.Name;
            sourceFileName = Path.GetFileName(sourceFilePath);
            outputFilePath = BuildOutputPath(outFilePath);
            debugFilePath = Compiler.DebugDirectory +
                            Path.GetFileNameWithoutExtension(outputFilePath) +
                            debugExtension;

            // initialize content
            SyntaxTree = new Ast(this);
            Lines = File.ReadAllText(file.FullName).Split(
                Spec.Newlines,
                StringSplitOptions.None
            );
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

        // TODO correct sourceFileName usages, resolve constructors difference.
        /// <summary>
        ///     Creates new <see cref="SourceCode" /> instance
        ///     using specified &lt;<paramref name="sourceLines" />&gt;.
        /// </summary>
        public SourceCode(string[] sourceLines, string outFilePath = null) {
            // initialize file paths
            sourceFilePath = Compiler.OutputDirectory + DateTime.Now.ToFileName() + Compiler.SourceFileExtension;
            sourceFileName = Path.GetFileName(sourceFilePath);
            outputFilePath = BuildOutputPath(outFilePath);
            debugFilePath = Compiler.DebugDirectory +
                            Path.GetFileNameWithoutExtension(outputFilePath) +
                            debugExtension;

            // initialize content
            SyntaxTree = new Ast(this);
            Lines      = sourceLines;
        }

        /// <summary>
        ///     Performs <see cref="SourceCode" /> processing
        ///     due to <see cref="mode" /> and <see cref="options" />.
        /// </summary>
        public void Process(SourceProcessingMode mode, SourceProcessingOptions options = SourceProcessingOptions.None) {
            Options = options;
            ConsoleLog.Info($"# Compiling '{sourceFileName}'");
            if (Lines.Length == 0) {
                ConsoleLog.Error("Source is empty. Compilation aborted.");
                goto COMPILATION_END;
            }
            // [1] Tokenizing
            ConsoleLog.Info("[*] Tokens list generation");
            {
                CorrectFormat(Lines);
                new Lexer(Lines, out Tokens, out Errors, Options).Process();
                if (mode == SourceProcessingMode.Lex) {
                    goto COMPILATION_END;
                }
            }
            // [2] Parsing
            ConsoleLog.Info("[*] Abstract Syntax Tree generation");
            {
                // new Parser(this).Process();
                if (mode == SourceProcessingMode.Parsing) {
                    goto COMPILATION_END;
                }
            }
            // [3] Code generation
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
                ConsoleLog.Info($"[*] Saving debugging information to '{debugFilePath}'");
                SaveDebugInfoToFile(debugFilePath, Tokens, SyntaxTree);
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
        private static void CorrectFormat(string[] lines) {
            // append newline statements to all lines except last
            for (var i = 0; i < lines.Length - 1; i++) {
                lines[i] += Spec.EndLine;
            }
        }

        private string BuildOutputPath(string outFilePath) {
            if (string.IsNullOrWhiteSpace(outFilePath)) {
                outFilePath =  Path.GetFileNameWithoutExtension(sourceFilePath) + Compiler.OutputFileExtension;
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
                "{" + Environment.NewLine +
                "\"tokens\": " +
                JsonConvert.SerializeObject(tokens, Compiler.JsonSerializer) +
                "," + Environment.NewLine +
                "\"syntaxTree\": " +
                JsonConvert.SerializeObject(syntaxTree, Compiler.JsonSerializer) +
                Environment.NewLine +
                "}";
            File.WriteAllText(debugPath, debugInfo);
        }
    }
}