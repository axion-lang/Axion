using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Axion.Core.Processing;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical;
using Axion.Core.Visual;
using CommandLine;
using FluentConsole;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace Axion.Core {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        /// <summary>
        ///     Main settings of JSON debug information formatting.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializer = new JsonSerializerSettings {
            Formatting           = Formatting.Indented,
            TypeNameHandling     = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        public static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where generated output is located.
        /// </summary>
        public static readonly string OutputDirectory = WorkDirectory + "output\\";

        /// <summary>
        ///     <see cref="Assembly" /> of <see cref="Core" /> namespace.
        /// </summary>
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public static readonly string Version = assembly.GetName().Version.ToString();

        private static readonly Parser cliParser = new Parser(
            settings => {
                settings.EnableDashDash = true;
                settings.CaseSensitive  = false;
                settings.HelpWriter     = null;
            }
        );

        private const string helpHint =
            "Type '-h', or '--help' to get documentation about launch arguments.";

        internal static FileInfo[] InputFiles { get; private set; }
        internal static bool       Verbose;

        public static void Init(string[] arguments) {
            if (!Directory.Exists(OutputDirectory)) {
                Directory.CreateDirectory(OutputDirectory);
            }

            PrintIntro();

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    cliParser
                        .ParseArguments<CommandLineArguments>(arguments)
                        .MapResult(
                            options => {
                                Verbose = options.Verbose;

                                if (options.Exit) {
                                    Environment.Exit(0);
                                }

                                if (options.ClearScreen) {
                                    Console.Clear();
                                    PrintIntro();
                                    return 0;
                                }

                                if (options.Version) {
                                    ConsoleUI.WriteLine(Version);
                                    return 0;
                                }

                                if (options.Help) {
                                    ConsoleUI.WriteLine(CommandLineArguments.HelpText);
                                    return 0;
                                }

                                if (options.Interactive) {
                                    EnterInteractiveMode();
                                    return 0;
                                }

                                ProcessSources(options);
                                return 0;
                            },
                            errors => {
                                foreach (Error e in errors) {
                                    Logger.Error(e.ToString());
                                }

                                return 0;
                            }
                        );
                }

                // wait for next command
                // TODO (UI) add console commands history (up-down)
                string command = ConsoleUI.Read(">>> ");
                while (command.Length == 0) {
                    ConsoleUI.ClearLine();
                    command = ConsoleUI.Read(">>> ");
                }

                ConsoleUI.WriteLine();
                arguments = Utilities.SplitLaunchArguments(command).ToArray();
            }

            // It is infinite loop, breaks only by 'exit' command.
            // ReSharper disable once FunctionNeverReturns
        }

        private static void PrintIntro() {
            Console.InputEncoding   = Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header; // set title
            ConsoleUI.WriteLine(
                (header + " v. ", ConsoleColor.White),
                (Version, ConsoleColor.DarkYellow)
            ); // print version
            ConsoleUI.WriteLine(
                ("Working in ", ConsoleColor.White),
                (WorkDirectory, ConsoleColor.DarkYellow)
            ); // print directory
            ConsoleUI.WriteLine(helpHint + "\n");
        }

        private static void EnterInteractiveMode() {
            Logger.Info(
                "Interactive mode.\n"
                + "Now your input will be processed by Axion interpreter.\n"
                + "Type 'exit' or 'quit' to quit interactive mode;\n"
                + "Type 'cls' to clear screen."
            );
            while (true) {
                // code editor header
                string input        = ConsoleUI.Read("i>> ");
                string alignedInput = input.Trim().ToUpper();

                switch (alignedInput) {
                    case "":
                        // skip empty commands
                        ConsoleUI.ClearLine();
                        continue;
                    case "EXIT":
                    case "QUIT":
                        // exit from interpreter to main loop
                        Logger.Info("\nInteractive interpreter closed.");
                        return;
                }

                // TODO (UI) parse "help(module)" argument

                // initialize editor
                var editor = new ConsoleCodeEditor(
                    false,
                    true,
                    firstCodeLine: input,
                    highlighter: new AxionSyntaxHighlighter()
                );
                string[] codeLines = editor.RunSession();
                // interpret as source code and output result
                Process(new SourceUnit(codeLines), SourceProcessingMode.Interpret);
            }
        }

        private static void ProcessSources(CommandLineArguments options) {
            SourceUnit source;
            // get source code
            if (options.Files.Any()) {
                int filesCount = options.Files.Count();
                if (filesCount > 1) {
                    Logger.Error("Compiler doesn't support multiple files processing yet.");
                    return;
                }

                InputFiles = new FileInfo[filesCount];
                for (var i = 0; i < filesCount; i++) {
                    InputFiles[i] = new FileInfo(
                        Utilities.TrimMatchingChars(options.Files.ElementAt(i), '"')
                    );
                }

                source = new SourceUnit(InputFiles[0]);
            }
            else if (!string.IsNullOrWhiteSpace(options.Code)) {
                source = new SourceUnit(Utilities.TrimMatchingChars(options.Code, '"'));
            }
            else {
                Logger.Error("Neither code nor path to source file not specified.\n" + helpHint);
                return;
            }

            if (!Enum.TryParse(options.Mode, true, out SourceProcessingMode processingMode)) {
                processingMode = SourceProcessingMode.Compile;
            }

            var processingOptions = SourceProcessingOptions.CheckIndentationConsistency;
            if (options.Debug) {
                processingOptions |= SourceProcessingOptions.SyntaxAnalysisDebugOutput;
            }

            if (options.AstJson) {
                processingOptions |= SourceProcessingOptions.ShowAstJson;
            }

            if (options.Rewrite) {
                processingOptions |= SourceProcessingOptions.RewriteFromAst;
            }

            // process source
            Process(source, processingMode, processingOptions);
        }

        public static SourceProcessingOptions Options { get; set; }

        /// <summary>
        ///     Performs [<see cref="SourceUnit" />] processing
        ///     with [<see cref="mode" />] and [<see cref="options" />].
        /// </summary>
        public static void Process(
            SourceUnit              unit,
            SourceProcessingMode    mode,
            SourceProcessingOptions options = SourceProcessingOptions.None
        ) {
            unit.ProcessingMode = mode;
            unit.Options        = Options = options;
            Process(unit);
            unit.ProcessingMode = SourceProcessingMode.None;
            unit.Options        = Options = SourceProcessingOptions.None;
        }

        private static void Process(SourceUnit unit) {
            Logger.Task($"Processing '{unit.SourceFileName}'");

            if (string.IsNullOrWhiteSpace(unit.Code)) {
                Logger.Error("Source is empty. Processing aborted.");
                FinishProcessing(unit);
                return;
            }

            // [1] Lexical analysis
            if (Verbose) {
                Logger.Step("Tokens list generation");
            }

            new Lexer(unit).Process();
            if (unit.ProcessingMode == SourceProcessingMode.Lex) {
                FinishProcessing(unit);
                return;
            }

            // [2] Parsing
            if (Verbose) {
                Logger.Step("Abstract Syntax Tree generation");
            }

            unit.Ast.Parse();
            if (unit.ProcessingMode == SourceProcessingMode.Parsing) {
                FinishProcessing(unit);
                return;
            }

            // [n] Code generation
            GenerateCode(unit);
            FinishProcessing(unit);
        }

        private static async void GenerateCode(SourceUnit unit) {
            if (unit.Blames.Any(e => e.Severity == BlameSeverity.Error)) {
                return;
            }

            switch (unit.ProcessingMode) {
                case SourceProcessingMode.Interpret: {
                    Logger.Task("Interpretation");
                    try {
                        string csCode = unit.Ast.ToCSharp().ToFullString();
                        if (unit.Options.HasFlag(
                            SourceProcessingOptions.SyntaxAnalysisDebugOutput
                        )) {
                            Logger.Step("Converter debug output:");
                            Logger.Log(csCode);
                        }

                        Logger.Step("Program output:");
                        ScriptState result = await CSharpScript.RunAsync(
                            csCode,
                            // Add Linq reference
                            ScriptOptions.Default.AddReferences(typeof(Enumerable).Assembly)
                        );
                        ConsoleUI.WriteLine();
                        Logger.Step("Result: " + (result.ReturnValue ?? "<nothing>"));
                    }
                    catch (CompilationErrorException e) {
                        Logger.Error(string.Join(Environment.NewLine, e.Diagnostics));
                    }

                    break;
                }

                case SourceProcessingMode.ConvertCS: {
                    Logger.Warn("Conversion to 'C#' is not fully implemented yet");
                    Logger.Log(unit.Ast.ToCSharp().ToFullString());
                    break;
                }

                case SourceProcessingMode.ConvertC: {
                    Logger.Error("Conversion to 'C' is not implemented yet");
                    break;
                }

                default: {
                    Logger.Error($"'{unit.ProcessingMode:G}' mode not implemented yet");
                    break;
                }
            }
        }

        private static void FinishProcessing(SourceUnit unit) {
            if (unit.Options.HasFlag(SourceProcessingOptions.ShowAstJson)) {
                Logger.Log(AstToMinifiedJson(unit));
            }

            if (unit.Options.HasFlag(SourceProcessingOptions.RewriteFromAst)) {
                Logger.Log(AstBackToSource(unit));
            }

            if (unit.Options.HasFlag(SourceProcessingOptions.SyntaxAnalysisDebugOutput)) {
                Logger.Step($"Saving debugging information to '{unit.DebugFilePath}'");
                SaveDebugInfoToFile(unit);
            }


            var errCount = 0;
            foreach (LanguageException e in unit.Blames) {
                e.Print(unit);
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

        #region Debug information

        /// <summary>
        ///     Saves processed source debug
        ///     information in JSON format.
        /// </summary>
        private static void SaveDebugInfoToFile(SourceUnit unit) {
            File.WriteAllText(
                unit.DebugFilePath,
                JsonConvert.SerializeObject(unit, JsonSerializer)
            );
        }

        /// <summary>
        ///     Prints generated AST in JSON format to console.
        /// </summary>
        private static string AstToMinifiedJson(SourceUnit unit) {
            string json = JsonConvert.SerializeObject(unit.Ast, JsonSerializer);
            json = Regex.Replace(json, @"\$type.+?(\w+?),.*\""", "$type\": \"$1\"");
            json = json.Replace("  ", "   ");
            return json;
        }

        private static string AstBackToSource(SourceUnit unit) {
            var code = new CodeBuilder(OutLang.Axion);
            unit.Ast.ToAxionCode(code);
            return code.ToString();
        }

        #endregion
    }
}