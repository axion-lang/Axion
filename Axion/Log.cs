using System;
using Axion.Processing;

namespace Axion {
    /// <summary>
    ///     Advanced static methods
    ///     wrapped around <see cref="Console" /> class.
    /// </summary>
    internal static class Log {
        internal const string HelpHint = "Type '-?' or '--help' to get documentation about launch arguments.";

        internal static void PrintCaption() {
            Console.ForegroundColor = ConsoleColor.White;
            const string header = "Axion programming language compiler toolset";
            Console.Title = header;                                                      // print title
            WriteLine(header + " v. ", Compiler.Version,       ConsoleColor.DarkYellow); // print version
            WriteLine("Working in ",   Compiler.WorkDirectory, ConsoleColor.DarkYellow); // print directory
            Console.WriteLine(HelpHint + Environment.NewLine);
        }

        internal static void Error(string message) {
            ConsoleColor prevFont = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ForegroundColor = prevFont;
        }

        internal static void Warn(ErrorType errorType, (int line, int column) position) {
            ConsoleColor prevFont = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Warning: {errorType:G}");
            Console.WriteLine($"At line {position.line}, column {position.column}.");
            Console.ForegroundColor = prevFont;
        }

        internal static void Info(string message) {
            ConsoleColor prevFont = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
            Console.ForegroundColor = prevFont;
        }

        internal static string ReadLine(string prompt, ConsoleColor inputColor = ConsoleColor.White) {
            Console.Write(prompt);
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = inputColor;
            string line = Console.ReadLine();
            Console.ForegroundColor = prevColor;
            return line;
        }

        internal static string Read(string prompt, ConsoleColor inputColor = ConsoleColor.White) {
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

        /// <summary>
        ///     Clears specified <paramref name="length" /> of current line.
        /// </summary>
        internal static void ClearLine(int length) {
            Console.CursorLeft = 0;
            var spaces = "";
            for (var _ = 0; _ < length; _++) {
                spaces += " ";
            }
            Console.Write(spaces);
        }

        /// <summary>
        ///     Clears current line.
        /// </summary>
        internal static void ClearLine() {
            int top = Console.CursorTop;
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, top);
        }

        #region Write() function

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="msg1Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            ConsoleColor msg1Color = ConsoleColor.White,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = msg1Color;
            Console.Write(message1);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />
        ///     and <paramref name="message2" /> with <paramref name="message2Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     <paramref name="message3" /> with <paramref name="message3Color" />,
        ///     and <paramref name="message4" /> with <paramref name="message4Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            string       message4,
            ConsoleColor message4Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = message4Color;
            Console.Write(message4);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     <paramref name="message3" /> with <paramref name="message3Color" />,
        ///     <paramref name="message4" /> with <paramref name="message4Color" />,
        ///     and <paramref name="message5" /> with <paramref name="message5Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            string       message4,
            ConsoleColor message4Color,
            string       message5,
            ConsoleColor message5Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = message4Color;
            Console.Write(message4);

            Console.ForegroundColor = message5Color;
            Console.Write(message5);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        #endregion

        #region Write(msg, msg, color) function

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     and <paramref name="message2" /> with <paramref name="message2Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            string       message2,
            ConsoleColor message2Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     <paramref name="message2" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void Write(
            string       message1,
            string       message2,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        #endregion

        #region WriteLine() function

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />with <paramref name="msg1Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            ConsoleColor msg1Color = ConsoleColor.White,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = msg1Color;
            Console.WriteLine(message1);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />
        ///     and <paramref name="message2" /> with <paramref name="message2Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.WriteLine(message2);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.WriteLine(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     <paramref name="message3" /> with <paramref name="message3Color" />,
        ///     and <paramref name="message4" /> with <paramref name="message4Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            string       message4,
            ConsoleColor message4Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = message4Color;
            Console.Write(message4);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" /> with <paramref name="message1Color" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     <paramref name="message3" /> with <paramref name="message3Color" />,
        ///     <paramref name="message4" /> with <paramref name="message4Color" />,
        ///     and <paramref name="message5" /> with <paramref name="message5Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            ConsoleColor message1Color,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            string       message4,
            ConsoleColor message4Color,
            string       message5,
            ConsoleColor message5Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.ForegroundColor = message1Color;
            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.Write(message3);

            Console.ForegroundColor = message4Color;
            Console.Write(message4);

            Console.ForegroundColor = message5Color;
            Console.WriteLine(message5);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        #endregion

        #region WriteLine(msg, msg, color) function

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     and <paramref name="message2" /> with <paramref name="message2Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            string       message2,
            ConsoleColor message2Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.WriteLine(message2);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     <paramref name="message2" /> with <paramref name="message2Color" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            string       message2,
            ConsoleColor message2Color,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.ForegroundColor = message2Color;
            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.WriteLine(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        /// <summary>
        ///     Writes a
        ///     <paramref name="message1" />,
        ///     <paramref name="message2" />,
        ///     and <paramref name="message3" /> with <paramref name="message3Color" />
        ///     and <paramref name="backColor" /> to the console.
        /// </summary>
        internal static void WriteLine(
            string       message1,
            string       message2,
            string       message3,
            ConsoleColor message3Color,
            ConsoleColor backColor = ConsoleColor.Black
        ) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.BackgroundColor = backColor;

            Console.Write(message1);

            Console.Write(message2);

            Console.ForegroundColor = message3Color;
            Console.WriteLine(message3);

            Console.ForegroundColor = prevFont;
            Console.BackgroundColor = prevBack;
        }

        #endregion

        #region Syntax highlighting

        #endregion
    }
}