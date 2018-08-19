using System;
using Axion.Processing;

namespace Axion.Visual {
    public class ConsoleLog {
        private static readonly ConsoleOutput output = (ConsoleOutput) ConsoleView.Output;

        public void Info(string message) {
            output.WriteLine((message, ConsoleColor.Blue));
        }

        public void Warn(ErrorType errorType, (int line, int column) position) {
            output.WriteLines(
                ($"Warning: {errorType:G}", ConsoleColor.DarkYellow),
                ($"At line {position.line}, column {position.column}.", ConsoleColor.DarkYellow)
            );
        }

        public void Error(string message) {
            output.WriteLine(($"Error: {message}", ConsoleColor.Red));
        }
    }
}