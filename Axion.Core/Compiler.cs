using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Axion.Core.Processing;
using Axion.Core.Visual;
using Axion.Core.Visual.ConsoleImpl;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Axion.Core {
    /// <summary>
    ///     Main class to work with Axion source code.
    /// </summary>
    public static class Compiler {
        internal const string HelpHint = "Type '-?', '-h', or '--help' to get documentation about launch arguments.";

        /// <summary>
        ///     Path to directory where compiler executable is located.
        /// </summary>
        internal static readonly string WorkDirectory = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        ///     Path to directory where sources debugging output is located.
        /// </summary>
        internal static readonly string DebugDirectory = WorkDirectory + "output\\";

        /// <summary>
        ///     Compiler version.
        /// </summary>
        internal static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        ///     Main settings of JSON debug information formatting.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializer =
            new JsonSerializerSettings { Formatting = Formatting.Indented };

        /// <summary>
        ///     Compiler files to process.
        /// </summary>
        internal static FileInfo[] InputFiles;

        public static void UnitTest(SourceCode code, SourceProcessingMode mode, SourceProcessingOptions options = SourceProcessingOptions.None) {
            code.Process(mode, options);
        }

        public static void Init(string[] arguments) {
            if (Directory.Exists(DebugDirectory)) {
                Directory.CreateDirectory(DebugDirectory);
            }

            ConsoleUI.Initialize();

            // main processing loop
            while (true) {
                if (arguments.Length > 0) {
                    try {
                        CommandLineArguments.App.Execute(arguments);
                    }
                    catch (CommandParsingException ex) {
                        ConsoleLog.Error(ex.Message);
                    }
                }
                // wait for next command
                string command = ConsoleUI.Read(">>> ");
                while (command.Length == 0) {
                    ConsoleUI.ClearLine();
                    command = ConsoleUI.Read(">>> ");
                }
                ConsoleUI.WriteLine();
                arguments = GetUserArguments(command).ToArray();
            }
            // It is infinite loop, breaks only by 'exit' command.
            // ReSharper disable once FunctionNeverReturns
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