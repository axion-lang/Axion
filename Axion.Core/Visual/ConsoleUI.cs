using System;
using System.Text;

namespace Axion.Core.Visual {
    /// <summary>
    ///     Implementation of Axion toolset UI using <see cref="Console" /> class.
    /// </summary>
    internal static class ConsoleUI {
        public static void Initialize() {
            Console.InputEncoding   = Console.OutputEncoding = Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;                                                                              // set title
            WriteLine((header + " v. ", ConsoleColor.White), (Compiler.Version, ConsoleColor.DarkYellow));       // print version
            WriteLine(("Working in ", ConsoleColor.White),   (Compiler.WorkDirectory, ConsoleColor.DarkYellow)); // print directory
            WriteLine(Compiler.HelpHint + "\n");
        }

        public static string Read(string prompt, ConsoleColor withColor = ConsoleColor.White) {
            var result = "";
            WithFontColor(
                withColor, () => {
                    var editor = new ConsoleCodeEditor(true, false, prompt);
                    result = editor.BeginSession()[0];
                }
            );
            return result;
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        public static void ClearLine(int fromX = 0) {
            Console.CursorLeft = fromX;
            Console.Write(new string(' ', Console.BufferWidth - fromX - 1));
            Console.CursorLeft = fromX;
        }

        #region Simple logging functions

        public static void LogInfo(string message) {
            WriteLine((message, ConsoleColor.DarkCyan));
        }

        public static void LogError(string message) {
            WriteLine(($"Error: {message}", ConsoleColor.Red));
        }

        #endregion

        #region Basic write functions

        /// <summary>
        ///     Writes messages to the standard output stream.
        /// </summary>
        public static void Write(params string[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                Console.Write(messages[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages to the standard output stream.
        /// </summary>
        public static void Write(params (string text, ConsoleColor color)[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                // ReSharper disable once AccessToModifiedClosure
                WithFontColor(messages[i].color, () => { Console.Write(messages[i].text); });
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
        public static void WriteLine(params string[] messages) {
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
        public static void WriteLine(params (string text, ConsoleColor color)[] messages) {
            for (var i = 0; i < messages.Length; i++) {
                if (i == messages.Length - 1) {
                    WithFontColor(messages[i].color, () => { Console.WriteLine(messages[i].text); });
                    return;
                }
                // ReSharper disable once AccessToModifiedClosure
                WithFontColor(messages[i].color, () => { Console.Write(messages[i].text); });
            }
        }

        /// <summary>
        ///     Writes messages with line terminators to the standard output stream.
        /// </summary>
        public static void WriteLines(params string[] lines) {
            for (var i = 0; i < lines.Length; i++) {
                Console.WriteLine(lines[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages with line terminators to the standard output stream.
        /// </summary>
        public static void WriteLines(params (string text, ConsoleColor color)[] lines) {
            for (var i = 0; i < lines.Length; i++) {
                // ReSharper disable once AccessToModifiedClosure
                WithFontColor(lines[i].color, () => { Console.WriteLine(lines[i].text); });
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        ///     Performs action in <see cref="Console" />, then returns
        ///     back to previous cursor position in <see cref="Console" />.
        /// </summary>
        internal static void WithCurrentPosition(Action action) {
            // save position
            int sX = Console.CursorLeft;
            int sY = Console.CursorTop;
            // do action
            action();
            // reset cursor
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Moves to specified <see cref="Console" /> position,
        ///     performs action, then returns back to previous
        ///     cursor position in <see cref="Console" />.
        /// </summary>
        internal static void WithPosition(int x, int y, Action action) {
            // save position
            int sX = Console.CursorLeft;
            int sY = Console.CursorTop;
            // move cursor
            Console.SetCursorPosition(x, y);
            // do action
            action();
            // reset cursor
            Console.SetCursorPosition(sX, sY);
        }

        /// <summary>
        ///     Sets <see cref="Console.ForegroundColor" /> to &lt;<see cref="color" />&gt;,
        ///     performs action, then returns back to previously used color.
        /// </summary>
        internal static void WithFontColor(ConsoleColor color, Action action) {
            // save color
            ConsoleColor prevColor = Console.ForegroundColor;
            // set new color
            Console.ForegroundColor = color;
            // do action
            action();
            // reset color
            Console.ForegroundColor = prevColor;
        }

        #endregion
    }
}