using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Processing.Syntactic;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Translation;
using Axion.Core.Processing.Traversal;
using Axion.Core.Specification;
using NLog;
using Module = Axion.Core.Hierarchy.Module;

namespace Axion.Core {
    public class Compiler {
        private static readonly Assembly coreAsm =
            Assembly.GetExecutingAssembly();

        public static readonly string Version =
            coreAsm.GetName().Version.ToString();

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        public static readonly string WorkDir =
            AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where generated output is located.
        /// </summary>
        public static readonly string OutDir = Path.Join(WorkDir, "output");

        public static readonly Dictionary<string, INodeTranslator> Translators = new();

        private static readonly Logger logger =
            LogManager.GetCurrentClassLogger();

        public static string GetTempSourceFilePath() {
            return Path.Combine(
                WorkDir,
                "temp",
                "tmp-" + DateTime.Now.ToFileName() + Spec.FileExtension
            );
        }

        public static void AddTranslator(
            string          name,
            INodeTranslator translator
        ) {
            Translators[name.Trim().ToLower()] = translator;
        }

        public static object? Process(
            Project           project,
            ProcessingOptions options
        ) {
            logger.Debug($"Processing project '{project.ConfigFile.Name}'");
            var result = Process(project.MainModule, options);
            return result;
        }

        public static object? Process(
            Module            module,
            ProcessingOptions options
        ) {
            logger.Debug(
                module.Parent == null
                    ? $"Processing module '{module.Name}'"
                    : $"Processing submodule '{module.Name}'"
            );
            object? result = null;
            foreach (var (_, subModule) in module.Submodules) {
                result = Process(subModule, options);
            }

            foreach (var (_, unit) in module.Units) {
                result = Process(unit, options);
            }

            return result;
        }

        public static object? Process(Unit src, ProcessingOptions options) {
            logger.Debug($"Processing '{src.SourceFile.Name}'");
            if (src.TextStream.IsEmpty) {
                logger.Error("Source is empty. Processing aborted.");
                return null;
            }

            object? result = null;
            foreach (var step in CompilationSteps) {
                result = step.Value(src, options);
                if (options.ProcessingMode == step.Key || src.HasErrors) {
                    break;
                }
            }

            var errCount = 0;
            foreach (var e in src.Blames) {
                logger.Error(e.ToString());
                if (e.Severity == BlameSeverity.Error) {
                    errCount++;
                }
            }

            logger.Debug(
                errCount > 0 ? "Processing aborted." : "Processing completed."
            );

            return result;
        }

        public static readonly Dictionary<Mode, Func<Unit, ProcessingOptions, object>>
            CompilationSteps = new() {
                { Mode.Lexing, Lex },
                { Mode.Parsing, Parse },
                { Mode.Reduction, Reduce },
                { Mode.Translation, Translate }
            };

        public static TokenStream Lex(Unit src, ProcessingOptions options) {
            logger.Debug("Tokens list generation");
            var lexer = new Lexer(src);
            while (true) {
                var token = lexer.Read();
                if (token == null) {
                    continue;
                }

                src.TokenStream.Add(token);
                if (lexer.ProcessTerminators.Contains(token.Type)) {
                    break;
                }
            }

            foreach (var mismatch in lexer.MismatchingPairs) {
                LanguageReport.MismatchedBracket(mismatch);
            }

            return src.TokenStream;
        }

        private static Ast Parse(Unit src, ProcessingOptions options) {
            logger.Debug("Abstract Syntax Tree generation");
            src.Ast.Parse();
            return src.Ast;
        }

        private static Ast Reduce(Unit src, ProcessingOptions options) {
            logger.Debug("Syntax tree reducing");
            Traversing.Traverse(src.Ast);
            return src.Ast;
        }

        private static CodeWriter Translate(
            Unit              src,
            ProcessingOptions options
        ) {
            if (!Translators.TryGetValue(
                options.TargetLanguage,
                out var ncv
            )) {
                logger.Error($"No frontend with name {options.TargetLanguage}");
                return CodeWriter.Default;
            }

            var cw = new CodeWriter(ncv);
            try {
                cw.Write(src.Ast);
                var code = cw.ToString();
                logger.Debug(code);
                File.WriteAllText(
                    Path.Combine(
                        src.OutputDirectory.FullName,
                        Path.ChangeExtension(
                            src.SourceFile.Name,
                            cw.OutputFileExtension
                        )
                    ),
                    code
                );
            }
            catch (Exception ex) {
                logger.Error("Translation failed:");
                logger.Info(ex.Message);
            }

            return cw;
        }
    }
}
