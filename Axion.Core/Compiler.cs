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
using Module = Axion.Core.Source.Module;

namespace Axion.Core {
    public class Compiler {
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

        internal readonly Module StdModule;

        public Compiler(string pathToStdLib) {
            try {
                StdModule = Module.Root(pathToStdLib);
            }
            catch (DirectoryNotFoundException e) {
                logger.Error(e.Message);
                return;
            }
            StdModule.AddSubmodule("macros").AddUnit("macros.ax");
        }

        public static void Process(
            Module            module,
            ProcessingMode    mode,
            ProcessingOptions options
        ) {
            module.ProcessingMode = mode;
            module.Options        = options;
            Process(src);
        }

        public static void Process(
            Unit              src,
            ProcessingMode    mode,
            ProcessingOptions options
        ) {
            src.ProcessingMode = mode;
            src.Options        = options;
            Process(src);
        }

        private static void Process(Module module) {

        }

        private static void Process(Unit src) {
            logger.Info($"Processing '{src.SourceFilePath.Name}'");
            if (string.IsNullOrWhiteSpace(src.TextStream.Text)) {
                logger.Error("Source is empty. Processing aborted.");
                return;
            }

            foreach ((ProcessingMode mode, Action<Unit> action) in CompilationSteps) {
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

            logger.Info(
                errCount > 0 ? "Processing terminated due to errors above" : "Processing finished"
            );
        }

        // @formatter:off

        public static readonly Dictionary<ProcessingMode, Action<Unit>> CompilationSteps =
            new Dictionary<ProcessingMode, Action<Unit>> {
                { ProcessingMode.Lexing, Lex },
                { ProcessingMode.Parsing, Parse },
                { ProcessingMode.Reduction, Reduce },
                { ProcessingMode.Transpilation, Transpile }
            };

        // @formatter:on

        public static void Lex(Unit src) {
            logger.Debug("Tokens list generation");
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
                LangException.ReportMismatchedBracket(mismatch);
            }
        }

        private static void Parse(Unit src) {
            logger.Debug("Abstract Syntax Tree generation");
            src.Ast.Parse();
        }

        private static void Reduce(Unit src) {
            logger.Debug("Syntax tree reducing");
            Traversing.Traverse(src.Ast);
        }

        private static void Transpile(Unit src) {
            try {
                src.CodeWriter.Write(src.Ast);
                var code = src.CodeWriter.ToString();
                logger.Debug("Transpiler output");
                logger.Debug(code);
                File.WriteAllText(src.OutputFilePath.FullName, code);
            }
            catch (Exception ex) {
                logger.Error("Transpiling failed:");
                logger.Info(ex.Message);
            }
        }
    }
}
