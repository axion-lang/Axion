using System;
using System.Collections.Generic;
using System.Text;
using Axion.Processing;

namespace Axion.Visual {
    /// <summary>
    ///     Implementation of Axion toolset UI using <see cref="Console" /> class.
    ///     Has instance type to simplify future expanding to IDE integration.
    /// </summary>
    internal static class ConsoleView {
        public static ConsoleCodeEditor Editor { get; private set; }

        public static readonly ConsoleLog Log = new ConsoleLog();

        public static readonly ConsoleOutput Output = new ConsoleOutput();

        public static void Initialize() {
            Console.InputEncoding   = Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;                                                                                     // print title
            Output.WriteLine((header + " v. ", ConsoleColor.White), (Compiler.Version, ConsoleColor.DarkYellow));       // print version
            Output.WriteLine(("Working in ", ConsoleColor.White),   (Compiler.WorkDirectory, ConsoleColor.DarkYellow)); // print directory
            Output.WriteLine(Compiler.HelpHint + Environment.NewLine);
        }

        public static void InteractiveMode() {
            Log.Info(
                "Interactive mode.\n" +
                "Now your input will be processed by Axion interpreter.\n" +
                "Type 'exit' or 'quit' to quit interactive mode;\n" +
                "Type 'cls' to clear screen."
            );
            while (true) {
                // code editor header
                string input        = Read("i>> ", ConsoleColor.Yellow);
                string alignedInput = input.Trim().ToUpper();
                // skip empty commands
                if (alignedInput == "") {
                    Output.ClearLine();
                    continue;
                }
                // exit from interpreter to main loop
                if (alignedInput == "EXIT" ||
                    alignedInput == "QUIT") {
                    Console.WriteLine();
                    Log.Info("Interactive interpreter closed.");
                    return;
                }
                if (alignedInput == "CLS") {
                    Console.Clear();
                    Initialize();
                    continue;
                }
                // TODO parse "help(module)" argument
                //if (alignedInput == "HELP" || alignedInput == "H" || alignedInput == "?") {
                //    // give help about some module/function.
                //    // should have control of all standard library documentation.
                //}

                Editor = new ConsoleCodeEditor(input);
                IEnumerable<string> codeLines = Editor.BeginSession();
                // interpret as Axion source and output result
                new SourceCode(codeLines).Process(SourceProcessingMode.Interpret);
            }
        }

        public static string Read(string prompt, ConsoleColor inputColor = ConsoleColor.White) {
            Console.Write(prompt);
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = inputColor;
            string         value = "";
            ConsoleKeyInfo key   = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter) {
                if (key.Key == ConsoleKey.Backspace) {
                    if (value.Length > 0) {
                        value = value.Remove(value.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else {
                    value += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
                key = Console.ReadKey(true);
            }
            Console.ForegroundColor = prevColor;
            return value;
        }

        public static string ReadLine(string prompt, ConsoleColor inputColor = ConsoleColor.White) {
            string line = Read(prompt, inputColor);
            Output.WriteLine();
            return line;
        }
    }
}