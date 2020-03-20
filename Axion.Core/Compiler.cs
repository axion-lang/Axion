using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;
using NLog;

namespace Axion.Core {
    public static class Compiler {
        private static readonly Assembly coreAsm = Assembly.GetExecutingAssembly();
        public static readonly  string   Version = coreAsm.GetName().Version.ToString();

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        public static readonly string WorkDir = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where generated output is located.
        /// </summary>
        public static readonly string OutDir = Path.Combine(WorkDir, "output");

        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Process(
            SourceUnit        src,
            ProcessingMode    mode,
            ProcessingOptions options = ProcessingOptions.Default
        ) {
            src.ProcessingMode = mode;
            src.Options        = options;
            Process(src);
        }

        private static void Process(SourceUnit src) {
            Logger.Info($"Processing '{src.SourceFilePath.Name}'");
            if (string.IsNullOrWhiteSpace(src.TextStream.Text)) {
                Logger.Error("Source is empty. Processing aborted.");
                return;
            }

            foreach ((ProcessingMode mode, Action<SourceUnit> action) in CompilationSteps) {
                action(src);
                if (src.ProcessingMode == mode || src.HasErrors) {
                    break;
                }
            }

            var errCount = 0;
            foreach (LangException e in src.Blames) {
                e.PrintToConsole();
                if (e.Severity == BlameSeverity.Error) {
                    errCount++;
                }
            }

            Logger.Info(
                errCount > 0
                    ? "Processing terminated due to errors above"
                    : "Processing finished"
            );
        }

        public static readonly Dictionary<ProcessingMode, Action<SourceUnit>> CompilationSteps =
            new Dictionary<ProcessingMode, Action<SourceUnit>> {
                { ProcessingMode.Lexing, Lex },
                { ProcessingMode.Parsing, Parse },
                { ProcessingMode.Reduction, Reduce },
                { ProcessingMode.Transpilation, Transpile }
            };

        public static void Lex(SourceUnit src) {
            Logger.Debug("Tokens list generation");
            var lexer = new Lexer(src);
            while (true) {
                Token? token = lexer.Read();
                if (token == null) {
                    continue;
                }

                src.TokenStream.Tokens.Add(token);
                if (lexer.ProcessTerminators.Contains(token.Type)) {
                    break;
                }
            }

            foreach (Token mismatch in lexer.MismatchingPairs) {
                LangException.Report(BlameType.MismatchedBracket, mismatch);
            }
        }

        private static void Parse(SourceUnit src) {
            Logger.Debug("Abstract Syntax Tree generation");
            src.Ast.Parse();
        }

        private static void Reduce(SourceUnit src) {
            Logger.Debug("Syntax tree reducing");
            Traversing.Traverse(src.Ast);
        }

        private static void Transpile(SourceUnit src) {
            try {
                src.CodeWriter.Write(src.Ast);
                var code = src.CodeWriter.ToString();
                Logger.Debug("Transpiler output");
                Logger.Debug(code);
                File.WriteAllText(src.OutputFilePath.FullName, code);
            }
            catch (Exception ex) {
                Logger.Error("Transpiling failed:");
                Logger.Info(ex.Message);
            }
        }
    }
}