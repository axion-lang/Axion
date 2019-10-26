using System;
using System.IO;
using System.Linq;
using System.Text;
using Axion.Core;
using Axion.Core.Source;
using CodeConsole;
using CodeConsole.CodeEditor;
using CommandLine;

namespace Axion {
    public static class Program {
        public static void Main(string[] arguments) {
            var cliParser = new Parser(
                settings => {
                    settings.EnableDashDash = true;
                    settings.CaseSensitive  = false;
                    settings.HelpWriter     = null;
                }
            );
            Directory.CreateDirectory(Compiler.OutDir);

            Console.InputEncoding   = Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            PrintIntro();

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

                                if (options.Cls) {
                                    Console.Clear();
                                    PrintIntro();
                                    return 0;
                                }

                                if (options.Version) {
                                    ConsoleUI.WriteLine(Compiler.Version);
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
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;
            ConsoleUI.WriteLine(
                (header + " v. ", ConsoleColor.White),
                (Compiler.Version, ConsoleColor.DarkYellow)
            );
            ConsoleUI.WriteLine(
                ("Working in ", ConsoleColor.White),
                (Compiler.WorkDir, ConsoleColor.DarkYellow)
            );
            ConsoleUI.WriteLine("Type '-h', or '--help' to get documentation about launch arguments.\n");
        }

        private static void EnterInteractiveMode() {
            Logger.Info(
                "Axion code editor & interpreter mode.\n"
              + "Type 'exit' or 'quit' to exit mode, 'cls' to clear screen."
            );
            while (true) {
                // code editor header
                string rawInput = ConsoleUI.Read("i>> ");
                string input    = rawInput.Trim().ToUpper();

                switch (input) {
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
                // initialize editor
                var      editor    = new CliEditor(firstCodeLine: rawInput);
                string[] codeLines = editor.Run();
                // interpret as source code and output result
                Compiler.Process(SourceUnit.FromLines(codeLines), ProcessingMode.Interpret);
            }
        }

        private static void ProcessSources(CommandLineArguments options) {
            SourceUnit src;
            if (options.Files.Any()) {
                int filesCount = options.Files.Count();
                if (filesCount > 1) {
                    Logger.Error("Compiler doesn't support multiple files processing yet.");
                    return;
                }

                var inputFiles = new FileInfo[filesCount];
                for (var i = 0; i < filesCount; i++) {
                    inputFiles[i] = new FileInfo(
                        Utilities.TrimMatchingChars(options.Files.ElementAt(i), '"')
                    );
                }

                src = SourceUnit.FromFile(inputFiles[0]);
            }
            else if (!string.IsNullOrWhiteSpace(options.Code)) {
                src = SourceUnit.FromCode(Utilities.TrimMatchingChars(options.Code, '"'));
            }
            else {
                Logger.Error("Neither code nor path to source file not specified.");
                return;
            }

            if (!Enum.TryParse(options.Mode, true, out ProcessingMode processingMode)) {
                processingMode = ProcessingMode.Compile;
            }

            var processingOptions = ProcessingOptions.CheckIndentationConsistency;
            if (options.Debug) {
                processingOptions |= ProcessingOptions.SyntaxAnalysisDebugOutput;
            }

            Compiler.Process(src, processingMode, processingOptions);
        }
    }
}