using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Processing;
using Axion.Visual;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Axion {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        internal const string HelpHint = "Type '-?', '-h', or '--help' to get documentation about launch arguments.";

        /// <summary>
        ///     Returns path to directory where compiler executable is located.
        /// </summary>
        internal static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Returns path to directory where sources debugging output is located.
        /// </summary>
        internal static readonly string DebugDirectory = WorkDirectory + "output\\";

        /// <summary>
        ///     Compiler version.
        /// </summary>
        internal const string Version = "0.2.9.92-alpha [unstable]";

        /// <summary>
        ///     Main settings of JSON debug information formatting.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializer =
            new JsonSerializerSettings { Formatting = Formatting.Indented };

        /// <summary>
        ///     Determines if compiler should save JSON debug information.
        /// </summary>
        public static bool Debug = false;

        /// <summary>
        ///     Compiler files to process.
        /// </summary>
        internal static FileInfo[] InputFiles;

        public static void UnitTest(SourceCode code, SourceProcessingMode mode) {
            code.Process(mode);
        }

        public static void Init(string[] arguments) {
            if (Directory.Exists(DebugDirectory)) {
                Directory.CreateDirectory(DebugDirectory);
            }

            ConsoleView.Initialize();
            
            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    try {
                        CommandLineArguments.Cli.Execute(arguments);
                    }
                    catch (CommandParsingException ex) {
                        ConsoleView.Log.Error(ex.Message);
                    }
                }
                // wait for next command
                string command = ConsoleView.Read(">>> ");
                while (command.Length == 0) {
                    ConsoleView.Output.ClearLine();
                    command = ConsoleView.Read(">>> ");
                }
                ConsoleView.Output.WriteLine();
                arguments = GetUserArguments(command).ToArray();
            }
            // It is infinite loop, breaks only by 'exit' command.
            // ReSharper disable once FunctionNeverReturns
        }

        public static class Options {
            /// <summary>
            ///     Determines if compiler should check that script use consistent indentation.
            ///     (e. g. only spaces or only tabs).
            /// </summary>
            public static bool CheckIndentationConsistency = true;

            public static int TabSize = 4;
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
                           return !inQuotes && Char.IsWhiteSpace(c);
                       }
                   )
                   .Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
                   .Where(arg => !String.IsNullOrEmpty(arg));
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