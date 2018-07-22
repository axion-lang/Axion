using System;
using Axion.Processing;

namespace Axion {
    /// <summary>
    ///     Advanced static methods
    ///     wrapped around <see cref="Console" /> class.
    /// </summary>
    internal static class Logger {
        /// <summary>
        ///     Writes a <paramref name="message" /> with specified <paramref name="fontColor" /> to the console.
        /// </summary>
        internal static void Colored(string       message,
                                     ConsoleColor fontColor = ConsoleColor.White,
                                     ConsoleColor backColor = ConsoleColor.Black) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.ForegroundColor = fontColor;
            Console.BackgroundColor = backColor;

            Console.Write(message);

            Console.BackgroundColor = prevBack;
            Console.ForegroundColor = prevFont;
        }

        /// <summary>
        ///     Writes a <paramref name="message" /> with specified <paramref name="fontColor" /> followed by new line to the
        ///     console.
        /// </summary>
        internal static void ColoredLine(string       message,
                                         ConsoleColor fontColor = ConsoleColor.White,
                                         ConsoleColor backColor = ConsoleColor.Black) {
            ConsoleColor prevBack = Console.BackgroundColor;
            ConsoleColor prevFont = Console.ForegroundColor;

            Console.ForegroundColor = fontColor;
            Console.BackgroundColor = backColor;

            Console.WriteLine(message);

            Console.BackgroundColor = prevBack;
            Console.ForegroundColor = prevFont;
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

        internal static string ReadLine(string prompt, ConsoleColor color = ConsoleColor.Yellow) {
            Console.Write(prompt);
            ConsoleColor prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            string line = Console.ReadLine();
            Console.ForegroundColor = prevColor;
            return line;
        }
    }
}