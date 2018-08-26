using System;
using Axion.Core.Processing;

namespace Axion.Core.Visual {
    public static class ConsoleLog {
        public static void Info(string message) {
            ConsoleUI.WriteLine((message, ConsoleColor.DarkCyan));
        }

        public static void Warn(ErrorType errorType, (int line, int column) position) {
            ConsoleUI.WriteLines(
                ($"Warning: {errorType:G}", ConsoleColor.DarkYellow),
                ($"At line {position.line}, column {position.column}.", ConsoleColor.DarkYellow)
            );
        }

        public static void Error(string message) {
            ConsoleUI.WriteLine(($"Error: {message}", ConsoleColor.Red));
        }
    }
}