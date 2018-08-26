using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Axion.Core.Processing;
using Axion.Core.Visual;
using CommandLine;
using Newtonsoft.Json;

namespace Axion.Core {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        internal const string HelpHint =
            "Type '-h', or '--help' to get documentation about launch arguments.";

        public const string SourceFileExtension = ".ax";
        public const string OutputFileExtension = ".ax";

        /// <summary>
        ///     Main settings of JSON debug information formatting.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializer = new JsonSerializerSettings {
            Formatting = Formatting.Indented
        };

        /// <summary>
        ///     <see cref="Assembly" /> of <see cref="Core" /> namespace.
        /// </summary>
        private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        internal static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where generated output is located.
        /// </summary>
        internal static readonly string OutputDirectory = WorkDirectory + "output\\";

        /// <summary>
        ///     Path to directory where debugging output is located.
        /// </summary>
        internal static readonly string DebugDirectory = OutputDirectory + "debug\\";

        /// <summary>
        ///     Compiler version.
        /// </summary>
        internal static readonly string Version = assembly.GetName().Version.ToString();

        /// <summary>
        ///     Compiler files to process.
        /// </summary>
        internal static FileInfo[] InputFiles;

        private static readonly Parser cliParser = new Parser(
            settings => {
                settings.EnableDashDash = true;
                settings.CaseSensitive  = false;
                settings.HelpWriter     = null;
            }
        );

        public static void Init(string[] arguments) {
            if (Directory.Exists(OutputDirectory)) {
                Directory.CreateDirectory(OutputDirectory);
            }

            ConsoleUI.Initialize();

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    cliParser
                        .ParseArguments<CommandLineOptions>(arguments)
                        .MapResult(
                            args => {
                                if (args.Exit) {
                                    Environment.Exit(0);
                                }
                                if (args.Version) {
                                    ConsoleUI.WriteLine(Version);
                                    return 0;
                                }
                                if (args.Help) {
                                    ConsoleUI.WriteLine(CommandLineOptions.HelpText);
                                    return 0;
                                }
                                // Set debug option
                                bool debugMode = args.Debug;
                                if (args.Interactive) {
                                    // Interactive mode: jump into interpreter processing loop
                                    ConsoleLog.Info(
                                        "Interactive mode.\n" +
                                        "Now your input will be processed by Axion interpreter.\n" +
                                        "Type 'exit' or 'quit' to quit interactive mode;\n" +
                                        "Type 'cls' to clear screen."
                                    );
                                    while (true) {
                                        // code editor header
                                        string input        = ConsoleUI.Read("i>> ");
                                        string alignedInput = input.Trim().ToUpper();

                                        if (alignedInput == "") {
                                            // skip empty commands
                                            ConsoleUI.ClearLine();
                                            continue;
                                        }
                                        if (alignedInput == "EXIT" ||
                                            alignedInput == "QUIT") {
                                            // exit from interpreter to main loop
                                            ConsoleUI.WriteLine();
                                            ConsoleLog.Info("Interactive interpreter closed.");
                                            return 0;
                                        }
                                        if (alignedInput == "CLS") {
                                            // clear screen
                                            Console.Clear();
                                            ConsoleUI.Initialize();
                                            continue;
                                        }
                                        // TODO parse "help(module)" argument
                                        //if (alignedInput.StartsWith("HELP")) {
                                        //    // give help about some module/function.
                                        //    // should have access to all standard library documentation.
                                        //}

                                        // initialize editor
                                        var      editor    = new ConsoleCodeEditor(false, true, "", input);
                                        string[] codeLines = editor.BeginSession();
                                        // interpret as Axion source and output result
                                        new SourceCode(codeLines).Process(SourceProcessingMode.Interpret);
                                    }
                                }
                                // process source
                                SourceCode source;
                                // get source code
                                if (args.Files.Any()) {
                                    int filesCount = args.Files.Count();
                                    if (filesCount > 1) {
                                        ConsoleLog.Error("Compiler doesn't support multiple files processing yet.");
                                        return 0;
                                    }
                                    InputFiles = new FileInfo[filesCount];
                                    for (var i = 0; i < filesCount; i++) {
                                        InputFiles[i] = new FileInfo(args.Files.ElementAt(i));
                                    }
                                    source = new SourceCode(InputFiles[0]);
                                }
                                else if (!string.IsNullOrWhiteSpace(args.Code)) {
                                    source = new SourceCode(Utilities.TrimMatchingQuotes(args.Code, '"'));
                                }
                                else {
                                    ConsoleLog.Error(
                                        "Neither code nor path to source file not specified.\n" +
                                        HelpHint
                                    );
                                    return 0;
                                }
                                if (!Enum.TryParse(args.Mode, true, out SourceProcessingMode processingMode)) {
                                    processingMode = SourceProcessingMode.Compile;
                                }
                                var processingOptions = SourceProcessingOptions.CheckIndentationConsistency;
                                if (debugMode) {
                                    processingOptions |= SourceProcessingOptions.SyntaxAnalysisDebugOutput;
                                }
                                // process source
                                source.Process(processingMode, processingOptions);
                                return 0;
                            },
                            errors => {
                                foreach (Error error in errors) {
                                    ConsoleLog.Error(error.ToString());
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
    }
}