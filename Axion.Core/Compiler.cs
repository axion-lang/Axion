using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Axion.Core.Processing;
using Axion.Core.Visual;
using CommandLine;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        public const string SourceFileExtension = ".ax";
        public const string OutputFileExtension = ".ax";

        /// <summary>
        ///     Main settings of JSON debug information formatting.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializer = new JsonSerializerSettings {
            Formatting = Formatting.Indented
        };

        internal const string HelpHint =
            "Type '-h', or '--help' to get documentation about launch arguments.";

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        internal static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where generated output is located.
        /// </summary>
        internal static readonly string OutputDirectory = WorkDirectory + "output\\";

        /// <summary>
        ///     <see cref="Assembly" /> of <see cref="Core" /> namespace.
        /// </summary>
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        ///     Compiler version.
        /// </summary>
        internal static readonly string Version = assembly.GetName().Version.ToString();

        /// <summary>
        ///     Compiler files to process.
        /// </summary>
        internal static FileInfo[] InputFiles { get; private set; }

        private static readonly Parser cliParser = new Parser(
            settings => {
                settings.EnableDashDash = true;
                settings.CaseSensitive  = false;
                settings.HelpWriter     = null;
            }
        );

        private static void PrintGreeting() {
            Console.InputEncoding   = Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;                                                                               // set title
            ConsoleUI.WriteLine((header + " v. ", ConsoleColor.White), (Version, ConsoleColor.DarkYellow));       // print version
            ConsoleUI.WriteLine(("Working in ", ConsoleColor.White),   (WorkDirectory, ConsoleColor.DarkYellow)); // print directory
            ConsoleUI.WriteLine(HelpHint + "\n");
        }

        public static void Init(string[] arguments) {
            if (!Directory.Exists(OutputDirectory)) {
                Directory.CreateDirectory(OutputDirectory);
            }

            PrintGreeting();

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    cliParser
                        .ParseArguments<CommandLineArguments>(arguments)
                        .MapResult(
                            options => {
                                if (options.Exit) {
                                    Environment.Exit(0);
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
                                foreach (Error error in errors) {
                                    ConsoleUI.LogError(error.ToString());
                                }
                                return 0;
                            }
                        );
                }
                // wait for next command
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

        private static void EnterInteractiveMode() {
            ConsoleUI.LogInfo(
                "Interactive mode.\n" +
                "Now your input will be processed by Axion interpreter.\n" +
                "Type 'exit' or 'quit' to quit interactive mode;\n" +
                "Type 'cls' to clear screen."
            );
            while (true) {
                // code editor header
                string input        = ConsoleUI.Read("i>> ");
                string alignedInput = input.Trim().ToUpper();

                if (alignedInput.Length == 0) {
                    // skip empty commands
                    ConsoleUI.ClearLine();
                    continue;
                }
                if (alignedInput == "EXIT"
                 || alignedInput == "QUIT") {
                    // exit from interpreter to main loop
                    ConsoleUI.WriteLine();
                    ConsoleUI.LogInfo("Interactive interpreter closed.");
                    return;
                }
                if (alignedInput == "CLS") {
                    // clear screen
                    Console.Clear();
                    PrintGreeting();
                    continue;
                }
                // TODO parse "help(module)" argument
                //if (alignedInput.StartsWith("HELP")) {
                //    // give help about some module/function.
                //    // should have access to all standard library documentation.
                //}

                // initialize editor
                var editor = new ConsoleCodeEditor(
                    false,
                    true,
                    firstCodeLine: input,
                    highlighter: new AxionSyntaxHighlighter()
                );
                string[] codeLines = editor.RunSession();
                // interpret as source code and output result
                new SourceCode(codeLines).Process(SourceProcessingMode.Interpret);
            }
        }

        private static void ProcessSources(CommandLineArguments options) {
            SourceCode source;
            // get source code
            if (options.Files.Any()) {
                int filesCount = options.Files.Count();
                if (filesCount > 1) {
                    ConsoleUI.LogError("Compiler doesn't support multiple files processing yet.");
                    return;
                }
                InputFiles = new FileInfo[filesCount];
                for (var i = 0; i < filesCount; i++) {
                    InputFiles[i] = new FileInfo(options.Files.ElementAt(i));
                }
                source = new SourceCode(InputFiles[0]);
            }
            else if (!string.IsNullOrWhiteSpace(options.Code)) {
                source = new SourceCode(Utilities.TrimMatchingChars(options.Code, '"'));
            }
            else {
                ConsoleUI.LogError(
                    "Neither code nor path to source file not specified.\n" +
                    HelpHint
                );
                return;
            }
            if (!Enum.TryParse(options.Mode, true, out SourceProcessingMode processingMode)) {
                processingMode = SourceProcessingMode.Compile;
            }
            var processingOptions = SourceProcessingOptions.CheckIndentationConsistency;
            if (options.Debug) {
                processingOptions |= SourceProcessingOptions.SyntaxAnalysisDebugOutput;
            }
            // process source
            source.Process(processingMode, processingOptions);
        }
    }
}