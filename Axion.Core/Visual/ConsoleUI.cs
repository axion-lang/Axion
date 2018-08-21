using System;
using System.Collections.Generic;
using System.Text;
using Axion.Core.Processing;

namespace Axion.Core.Visual.ConsoleImpl {
    /// <summary>
    ///     Implementation of Axion toolset UI using <see cref="Console" /> class.
    /// </summary>
    internal static class ConsoleUI {
        public static void Initialize() {
            Console.InputEncoding   = Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;                                                                              // print title
            WriteLine((header + " v. ", ConsoleColor.White), (Compiler.Version, ConsoleColor.DarkYellow));       // print version
            WriteLine(("Working in ", ConsoleColor.White),   (Compiler.WorkDirectory, ConsoleColor.DarkYellow)); // print directory
            WriteLine(Compiler.HelpHint + Environment.NewLine);
        }

        public static void InteractiveSession() {
            ConsoleLog.Info(
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
                    ClearLine();
                    continue;
                }
                // exit from interpreter to main loop
                if (alignedInput == "EXIT" ||
                    alignedInput == "QUIT") {
                    WriteLine();
                    ConsoleLog.Info("Interactive interpreter closed.");
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

                IEnumerable<string> codeLines = ConsoleCodeEditor.BeginSession(input);
                // interpret as Axion source and output result
                new SourceCode(codeLines).Process(SourceProcessingMode.Interpret);
            }
        }

        public static string Read(string prompt, ConsoleColor inputColor = ConsoleColor.White) {
            Console.Write(prompt);
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = inputColor;
            var            value = "";
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
            WriteLine();
            return line;
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        public static void ClearLine() {
            int top = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, top);
        }

        /// <summary>
        ///     Clears specified <paramref name="length" /> of current line.
        /// </summary>
        public static void ClearLine(int length) {
            Console.CursorLeft = 0;
            var spaces = "";
            for (var _ = 0; _ < length; _++) {
                spaces += " ";
            }
            Console.Write(spaces);
        }

        #region Basic write functions

        /// <summary>
        ///     Writes messages to the standard output stream.
        /// </summary>
        public static void Write(params object[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                Console.Write(messages[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages to the standard output stream.
        /// </summary>
        public static void Write(params (object text, ConsoleColor color)[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = messages[i].color;
                Console.Write(messages[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        /// <summary>
        ///     Writes line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine() {
            Console.WriteLine();
        }

        /// <summary>
        ///     Writes messages followed by last line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine(params object[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                if (i == messages.Length - 1) {
                    Console.WriteLine(messages[i]);
                    return;
                }
                Console.Write(messages[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages followed by last line terminator to the standard output stream.
        /// </summary>
        public static void WriteLine(params (object text, ConsoleColor color)[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                ConsoleColor prevColor;
                if (i == messages.Length - 1) {
                    prevColor               = Console.ForegroundColor;
                    Console.ForegroundColor = messages[i].color;
                    Console.WriteLine(messages[i].text.ToString());
                    Console.ForegroundColor = prevColor;
                    return;
                }
                prevColor               = Console.ForegroundColor;
                Console.ForegroundColor = messages[i].color;
                Console.Write(messages[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        /// <summary>
        ///     Writes messages with line terminators to the standard output stream.
        /// </summary>
        public static void WriteLines(params object[] lines) {
            for (var i = 0; i < lines.Length; i++) {
                Console.WriteLine(lines[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages with line terminators to the standard output stream.
        /// </summary>
        public static void WriteLines(params (object text, ConsoleColor color)[] lines) {
            for (var i = 0; i < lines.Length; i++) {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = lines[i].color;
                Console.WriteLine(lines[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        #endregion
    }
}