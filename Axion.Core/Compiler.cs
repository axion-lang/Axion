using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Traversal;
using Axion.Core.Source;
using Axion.Core.Specification;
using CodeConsole;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

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

        public const string SourceFileExt = ".ax";
        public const string OutputFileExt = ".out";
        public const string DebugFileExt  = ".dbg.json";

        public static FileInfo CreateTempSourcePath() {
            var path = new FileInfo(
                Path.Combine(
                    WorkDir,
                    "temp",
                    "tmp-" + DateTime.Now.ToFileName() + SourceFileExt
                ));
            Utilities.ResolvePath(path.Directory.FullName);
            return path;
        }

        public static void Process(
            SourceUnit        src,
            ProcessingMode    mode,
            ProcessingOptions options = ProcessingOptions.None
        ) {
            src.ProcessingMode = mode;
            src.Options        = options;
            Process(src);
            src.ProcessingMode = ProcessingMode.None;
            src.Options        = ProcessingOptions.None;
        }

        private static void Process(SourceUnit src) {
            Logger.Task($"Processing '{src.SourceFilePath.Name}'");

            if (string.IsNullOrWhiteSpace(src.TextStream.Text)) {
                Logger.Error("Source is empty. Processing aborted.");
                FinishProcessing(src);
                return;
            }

            Logger.Step("Tokens list generation");
            LexicalAnalysis(src);
            if (src.ProcessingMode == ProcessingMode.Lex) {
                FinishProcessing(src);
                return;
            }

            Logger.Step("Abstract Syntax Tree generation");
            src.Ast.Parse();
            if (src.ProcessingMode == ProcessingMode.Parsing) {
                FinishProcessing(src);
                return;
            }

            Logger.Step("Syntax tree reducing");
            Traversing.Traverse(src.Ast, Traversing.Walker);
            if (src.ProcessingMode == ProcessingMode.Traversing) {
                FinishProcessing(src);
                return;
            }

            GenerateCode(src);
            FinishProcessing(src);
        }

        public static void LexicalAnalysis(SourceUnit src) {
            while (true) {
                Token token = new Token(src).Read();
                if (token != null) {
                    src.TokenStream.Tokens.Add(token);
                    if (src.ProcessTerminators.Contains(token.Type)) {
                        break;
                    }
                }
            }

            foreach (Token mismatch in src.MismatchingPairs) {
                LangException.Report(BlameType.MismatchedBracket, mismatch);
            }
        }

        private static async void GenerateCode(SourceUnit src) {
            switch (src.ProcessingMode) {
            case ProcessingMode.Interpret: {
                try {
                    var cb = new CodeWriter(ProcessingMode.ConvertCS);
                    src.Ast.ToCSharp(cb);
                    string csCode = cb.ToString();
                    if (src.Options.HasFlag(ProcessingOptions.SyntaxAnalysisDebugOutput)) {
                        Logger.Step("Transpiler output:");
                        Logger.Log(csCode);
                    }

                    Logger.Task("Interpretation");
                    Logger.Step("Program output:");
                    ScriptState result = await CSharpScript.RunAsync(
                        csCode,
                        ScriptOptions.Default.AddReferences(Spec.CSharp.DefaultImports)
                    );
                    Logger.Step("\nResult: " + (result.ReturnValue ?? "<nothing>"));
                }
                catch (CompilationErrorException e) {
                    Logger.Error(string.Join(Environment.NewLine, e.Diagnostics));
                }

                break;
            }

            case ProcessingMode.ConvertCS: {
                Logger.Warn("Conversion to C# is not fully implemented yet");
                var cb = new CodeWriter(ProcessingMode.ConvertCS);
                cb.Write(src.Ast);
                string code = cb.ToString();
                Logger.Step("Transpiler output");
                Logger.Log(code);
                try {
                    Logger.Step("Program output:");
                    ScriptState result = await CSharpScript.RunAsync(
                        code,
                        ScriptOptions.Default.AddReferences(Spec.CSharp.DefaultImports)
                    );
                    Logger.Step("\nResult: " + (result.ReturnValue ?? "<nothing>"));
                }
                catch (CompilationErrorException e) {
                    Logger.Error(string.Join(Environment.NewLine, e.Diagnostics));
                }

                break;
            }
            default: {
                try {
                    var cb = new CodeWriter(src.ProcessingMode);
                    cb.Write(src.Ast);
                    string code = cb.ToString();
                    Logger.Step("Transpiler output");
                    Logger.Log(code);
                }
                catch (Exception ex) {
                    Logger.Error($"'{src.ProcessingMode:G}' failed:");
                    Logger.Log(ex.Message);
                }

                break;
            }
            }
        }

        private static void FinishProcessing(SourceUnit src) {
            if (src.Options.HasFlag(ProcessingOptions.SyntaxAnalysisDebugOutput)) {
                string astJson = AstToMinifiedJson(src);
                Logger.Info(astJson);
                Logger.Step($"Saving debug info --> {src.DebugFilePath}");
                if (!src.DebugFilePath.Exists) {
                    Utilities.ResolvePath(src.DebugFilePath.Directory.FullName);
                    using (src.DebugFilePath.Create()) { }
                }

                File.WriteAllText(src.DebugFilePath.FullName, astJson);
            }


            var errCount = 0;
            foreach (LangException e in src.Blames) {
                e.Print();
                if (e.Severity == BlameSeverity.Error) {
                    errCount++;
                }
            }

            Logger.Task(
                errCount > 0
                    ? "Processing terminated due to errors above"
                    : "Processing finished"
            );
        }

        /// <summary>
        ///     Serializes generated AST to minified-JSON format.
        /// </summary>
        public static string AstToMinifiedJson(SourceUnit src) {
            string json = JsonConvert.SerializeObject(src.Ast, JsonHelpers.Settings);
            json = Regex.Replace(json, @"\$type.+?(\w+?),.*\""", "$type\": \"$1\"");
            json = json.Replace("  ", "   ");
            return json;
        }
    }
}