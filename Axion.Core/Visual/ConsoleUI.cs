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
            Console.Title = header; // print title
            WriteLine(
                (header + " v. ", ConsoleColor.White), (Compiler.Version, ConsoleColor.DarkYellow)
            ); // print version
            WriteLine(
                ("Working in ", ConsoleColor.White), (Compiler.WorkDirectory, ConsoleColor.DarkYellow)
            ); // print directory
            WriteLine(Compiler.HelpHint + "\n");
        }

        public static string Read(string prompt, ConsoleColor withColor = ConsoleColor.White) {
            Console.Write(prompt);
            var result = "";
            DoWithFontColor(
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
        public static void ClearLine() {
            int top = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, top);
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
                // ReSharper disable once AccessToModifiedClosure
                DoWithFontColor(messages[i].color, () => { Console.Write(messages[i].text.ToString()); });
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
                if (i == messages.Length - 1) {
                    DoWithFontColor(messages[i].color, () => { Console.WriteLine(messages[i].text.ToString()); });
                    return;
                }
                // ReSharper disable once AccessToModifiedClosure
                DoWithFontColor(messages[i].color, () => { Console.Write(messages[i].text.ToString()); });
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
                // ReSharper disable once AccessToModifiedClosure
                DoWithFontColor(lines[i].color, () => { Console.WriteLine(lines[i].text.ToString()); });
            }
        }

        #endregion

        /// <summary>
        ///     Performs action in <see cref="Console" />, then returns
        ///     back to previous cursor position in <see cref="Console" />.
        /// </summary>
        internal static void DoAfterCursor(Action action) {
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
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="action">action to perform at position (<paramref name="x" />, <paramref name="y" />).</param>
        internal static void DoWithPosition(int x, int y, Action action) {
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
        internal static void DoWithFontColor(ConsoleColor color, Action action) {
            // save color
            ConsoleColor prevColor = Console.ForegroundColor;
            // set new color
            Console.ForegroundColor = color;
            // do action
            action();
            // reset color
            Console.ForegroundColor = prevColor;
        }
    }
}