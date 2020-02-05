using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Axion.Core.Processing.CodeGen;
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

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Process(
            SourceUnit        src,
            ProcessingMode    mode,
            ProcessingOptions options = ProcessingOptions.Default
        ) {
            src.ProcessingMode = mode;
            src.Options        = options;
            Process(src);
            src.ProcessingMode = ProcessingMode.None;
            src.Options        = ProcessingOptions.None;
        }

        private static void Process(SourceUnit src) {
            logger.Info($"Processing '{src.SourceFilePath.Name}'");
            if (string.IsNullOrWhiteSpace(src.TextStream.Text)) {
                logger.Error("Source is empty. Processing aborted.");
                return;
            }

            foreach ((ProcessingMode mode, Action<SourceUnit> action) in CompilationSteps) {
                action(src);
                if (src.ProcessingMode == mode) {
                    break;
                }
            }

            var errCount = 0;
            foreach (LangException e in src.Blames) {
                e.Print();
                if (e.Severity == BlameSeverity.Error) {
                    errCount++;
                }
            }

            logger.Info(
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
            logger.Debug("Tokens list generation");
            var lexer = new Lexer(src);
            while (true) {
                Token token = lexer.Read();
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
            logger.Debug("Abstract Syntax Tree generation");
            src.Ast.Parse();
        }

        private static void Reduce(SourceUnit src) {
            logger.Debug("Syntax tree reducing");
            Traversing.Traverse(src.Ast);
        }

        private static void Transpile(SourceUnit src) {
            try {
                src.CodeWriter = new CodeWriter(src.Options);
                src.CodeWriter.Write(src.Ast);
                var code = src.CodeWriter.ToString();
                logger.Debug("Transpiler output");
                logger.Debug(code);
                if (!(File.Exists(src.OutputFilePath.FullName) && src.OutputFilePath.Extension == ".ax")) {
                    File.WriteAllText(src.OutputFilePath.FullName, code);
                }
            }
            catch (Exception ex) {
                logger.Error("Transpiling failed:");
                logger.Info(ex.Message);
            }
        }
    }
}