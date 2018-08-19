using System;

namespace Axion.Visual {
    internal class ConsoleOutput {
        /// <summary>
        ///     Clears current line.
        /// </summary>
        public void ClearLine() {
            int top = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, top);
        }

        /// <summary>
        ///     Clears specified <paramref name="length" /> of current line.
        /// </summary>
        public void ClearLine(int length) {
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
        public void Write(params object[] messages) {
            for (int i = 0; i < messages.Length; i++) {
                Console.Write(messages[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages to the standard output stream.
        /// </summary>
        public void Write(params (object text, ConsoleColor color)[] messages) {
            for (int i = 0; i < messages.Length; i++) {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = messages[i].color;
                Console.Write(messages[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        /// <summary>
        ///     Writes line terminator to the standard output stream.
        /// </summary>
        public void WriteLine() {
            Console.WriteLine();
        }

        /// <summary>
        ///     Writes messages followed by last line terminator to the standard output stream.
        /// </summary>
        public void WriteLine(params object[] messages) {
            for (int i = 0; i < messages.Length; i++) {
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
        public void WriteLine(params (object text, ConsoleColor color)[] messages) {
            for (int i = 0; i < messages.Length; i++) {
                ConsoleColor prevColor;
                if (i == messages.Length - 1) {
                    prevColor = Console.ForegroundColor;
                    Console.ForegroundColor = messages[i].color;
                    Console.WriteLine(messages[i].text.ToString());
                    Console.ForegroundColor = prevColor;
                    return;
                }
                prevColor = Console.ForegroundColor;
                Console.ForegroundColor = messages[i].color;
                Console.Write(messages[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        /// <summary>
        ///     Writes messages with line terminators to the standard output stream.
        /// </summary>
        public void WriteLines(params object[] lines) {
            for (int i = 0; i < lines.Length; i++) {
                Console.WriteLine(lines[i]);
            }
        }

        /// <summary>
        ///     Writes colored messages with line terminators to the standard output stream.
        /// </summary>
        public void WriteLines(params (object text, ConsoleColor color)[] lines) {
            for (int i = 0; i < lines.Length; i++) {
                ConsoleColor prevColor = Console.ForegroundColor;
                Console.ForegroundColor = lines[i].color;
                Console.WriteLine(lines[i].text.ToString());
                Console.ForegroundColor = prevColor;
            }
        }

        #endregion
    }
}