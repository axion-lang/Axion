using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Processing;
using Fclp;
using Newtonsoft.Json;

namespace Axion {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        /// <summary>
        ///     Returns path to directory where compiler executable is located.
        /// </summary>
        internal static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private const string typeHelp = "Type '-?' or '--help' to get documentation about launch arguments.";
        private const string version  = "0.2.9.6-alpha [unstable]";

        /// <summary>
        ///     Compiler files to process.
        /// </summary>
        private static FileInfo[] inputFiles;

        public static void Process(SourceCode code, SourceProcessingMode mode) {
            code.Process(mode);
        }

        /// <summary>
        ///     Main compiler method that enters into infinite working loop
        ///     (input -> response).
        /// </summary>
        /// <param name="arguments">Command line arguments compiler was launched with.</param>
        public static void Launch(string[] arguments) {
            Log.WriteLine("Axion programming language compiler v. ", version, ConsoleColor.Yellow);
            Log.WriteLine("Working in ", WorkDirectory, ConsoleColor.Yellow);
            Console.WriteLine(typeHelp + Environment.NewLine);

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    FluentCommandLineParser<LaunchArguments> cliParser = InitCLIParser();
                    ICommandLineParserResult                 result    = cliParser.Parse(arguments);
                    // if launch arguments not modified
                    // TODO Synchronize launch arguments count with UnmatchedOptions
                    if (result.UnMatchedOptions.Count() == 8) {
                        Log.Error("Invalid argument.\n" + typeHelp);
                    }
                    else if (result.HasErrors) {
                        Log.Error(result.ErrorText);
                    }
                    else {
                        HandleLaunchArguments(cliParser.Object);
                    }
                }
                // wait for next command
                string command = Log.ReadLine(">>> ");
                arguments = GetUserArguments(command).ToArray();
            }
            // It is infinite loop, breaks only by 'exit' command.
            // ReSharper disable once FunctionNeverReturns
        }

        private static void HandleLaunchArguments(LaunchArguments args) {
            if (args.Exit) {
                Environment.Exit(0);
            }
            if (args.Version) {
                Console.WriteLine(version);
                return;
            }
            if (args.Help) {
                DisplayHelpScreen();
                return;
            }
            Options.Debug = args.Debug;

            // Interactive mode: jump into interpreter processing loop
            if (args.Interactive) {
                Log.Info(
                    "Interactive interpreter mode.\n" +
                    "Now your input will be processed by Axion interpreter.\n" +
                    "Type 'exit' to close interpreter environment;\n" +
                    "Type 'cls' to clear screen."
                );
                while (true) {
                    string input      = Log.ReadLine(">>> ");
                    string lowerInput = input.Trim().ToLower();
                    // skip empty commands
                    if (lowerInput == "") {
                        continue;
                    }
                    // exit from interpreter to main loop
                    if (lowerInput == "exit") {
                        Log.Info("Interactive interpreter closed.");
                        return;
                    }
                    if (lowerInput == "cls") {
                        Console.Clear();
                        continue;
                    }
                    var script = "";
                    while (!string.IsNullOrEmpty(input)) {
                        script += input + "\n";
                        input  =  Log.ReadLine("... ");
                    }
                    // TODO parse "help(module)" argument
                    //if (lowerInput == "help" || lowerInput == "h" || lowerInput == "?") {
                    //    // give help about some module/function.
                    //    // should have control about all standard library documentation.
                    //}

                    // interpret as Axion source and output result
                    new SourceCode(script).Process(SourceProcessingMode.Interpret);
                }
            }

            SourceCode source;
            // get source code
            if (args.Files != null) {
                if (args.Files.Count > 1) {
                    Log.Error("Compiler doesn't support multiple files processing yet.");
                    return;
                }
                inputFiles = new FileInfo[args.Files.Count];
                for (var i = 0; i < args.Files.Count; i++) {
                    inputFiles[i] = new FileInfo(args.Files[i]);
                }
                source = new SourceCode(inputFiles[0]);
            }
            else if (args.Script != null) {
                source = new SourceCode(args.Script);
            }
            else {
                Log.Error("Neither script nor path to script file not specified.\n" + typeHelp);
                return;
            }

            // process source
            source.Process(args.Mode);
        }

        /// <summary>
        ///     Initializes command line parser with allowed arguments.
        /// </summary>
        /// <returns>Ready parser instance.</returns>
        private static FluentCommandLineParser<LaunchArguments> InitCLIParser() {
            var cliParser = new FluentCommandLineParser<LaunchArguments> { IsCaseSensitive = false };
            cliParser.Setup(arg => arg.Files)
                     .As('f', nameof(LaunchArguments.Files));
            cliParser.Setup(arg => arg.Script)
                     .As('s', nameof(LaunchArguments.Script));
            cliParser.Setup(arg => arg.Mode)
                     .As('m', nameof(LaunchArguments.Mode));
            cliParser.Setup(arg => arg.Interactive)
                     .As('i', nameof(LaunchArguments.Interactive));
            cliParser.Setup(arg => arg.Debug)
                     .As('d', nameof(LaunchArguments.Debug));
            cliParser.Setup(arg => arg.Help)
                     .As('?', nameof(LaunchArguments.Help));
            cliParser.Setup(arg => arg.Version)
                     .As('v', nameof(LaunchArguments.Version));
            cliParser.Setup(arg => arg.Exit)
                     .As('x', nameof(LaunchArguments.Exit));
            return cliParser;
        }

        /// <summary>
        ///     Displays a table with documentation about launch arguments.
        /// </summary>
        private static void DisplayHelpScreen() {
            Console.Write(
                $@"
┌─────────────────────────────┬───────────────────────────────────────────────────────────────┐
│        Argument name        │                                                               │
├───────┬─────────────────────┤                       Usage description                       │
│ short │        full         │                                                               │
├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤
│  -i   │ --{nameof(LaunchArguments.Interactive)}       │ Launch compiler's interactive interpreter mode.               │
├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤
│  -f   │ --{nameof(LaunchArguments.Files)}=""<path>""    │ Input files to process.                                       │
│  -s   │ --{nameof(LaunchArguments.Script)}=""<code>""   │ Input script to process.                                      │
├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤
│  -m   │ --mode=<value>      │ Source code processing mode (Default: compile). Available:    ├──┬── not available yet
│       │ {nameof(SourceProcessingMode.Interpret)}           │     Interpret source code.                                    │  │
│       │ {nameof(SourceProcessingMode.Compile)}             │     Compile source into machine code.                         │  │
│       │ {nameof(SourceProcessingMode.ConvertC)}            │     Convert source to 'C' language.                           │  │
│       │ {nameof(SourceProcessingMode.ConvertCpp)}          │     Convert source to 'C++' language.                         │  │
│       │ {nameof(SourceProcessingMode.ConvertCSharp)}       │     Convert source to 'C#' language.                          │  │
│       │ {nameof(SourceProcessingMode.ConvertJavaScript)}   │     Convert source to 'JavaScript' language.                  │  │
│       │ {nameof(SourceProcessingMode.ConvertPython)}       │     Convert source to 'Python' language.                      ├──┘
│       │ {nameof(SourceProcessingMode.Lex)}                 │     Create tokens (lexemes) list from.                        │
│       │ {nameof(SourceProcessingMode.Parsing)}             │     Create tokens list and Abstract Syntax Tree from source.  │
├───────┼─────────────────────┼───────────────────────────────────────────────────────────────┤
│  -d   │ --{nameof(LaunchArguments.Debug)}             │ Save debug information to '<compilerDir>\output' directory.   │
│  -?   │ --{nameof(LaunchArguments.Help)}              │ Display this help screen.                                     │
│  -v   │ --{nameof(LaunchArguments.Version)}           │ Display information about compiler version.                   │
│  -x   │ --{nameof(LaunchArguments.Exit)}              │ Exit the compiler.                                            │
└───────┴─────────────────────┴───────────────────────────────────────────────────────────────┘
 (Argument names are not case-sensitive)
"
            );
        }

        public static class Options {
            /// <summary>
            ///     Main settings of JSON debug information formatting.
            /// </summary>
            public static readonly JsonSerializerSettings JsonSerializer =
                new JsonSerializerSettings { Formatting = Formatting.Indented };

            /// <summary>
            ///     Determines if compiler should save JSON debug information.
            /// </summary>
            public static bool Debug = true;

            /// <summary>
            ///     Determines if compiler should check that script use consistent indentation.
            ///     (e. g. only spaces or only tabs).
            /// </summary>
            public static bool CheckIndentationConsistency = true;
        }

        #region Get user input and split it into arguments

        /// <summary>
        ///     Splits user command line input to arguments.
        /// </summary>
        /// <returns>Collection of arguments passed into command line.</returns>
        private static IEnumerable<string> GetUserArguments(string input) {
            var inQuotes = false;
            return Split(
                       input, c => {
                                  if (c == '\"') {
                                      inQuotes = !inQuotes;
                                  }
                                  return !inQuotes && char.IsWhiteSpace(c);
                              }
                   )
                   .Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
                   .Where(arg => !string.IsNullOrEmpty(arg));
        }

        private static IEnumerable<string> Split(string str, Func<char, bool> controller) {
            var nextPiece = 0;
            for (var c = 0; c < str.Length; c++) {
                if (controller(str[c])) {
                    yield return str.Substring(nextPiece, c - nextPiece);

                    nextPiece = c + 1;
                }
            }
            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(string input, char quote) {
            if (input.Length >= 2
             && input[0] == quote
             && input[input.Length - 1] == quote) {
                return input.Substring(1, input.Length - 2);
            }
            return input;
        }

        #endregion
    }
}