using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Source;
using Axion.Core.Specification;
using CodeConsole;
using CodeConsole.CodeEditor;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    /// <summary>
    ///     Exception that occurs during invalid
    ///     Axion code processing.
    /// </summary>
    public class LangException : Exception {
        public override string        Message    { get; }
        public override string        StackTrace { get; }
        public          BlameSeverity Severity   { get; }

        [JsonProperty]
        private readonly Span errorSpan;

        [JsonProperty]
        private readonly SourceUnit targetSource;

        [JsonProperty]
        private readonly string time = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        private LangException(string message, BlameSeverity severity, Span span) {
            Severity     = severity;
            Message      = message;
            errorSpan    = span;
            targetSource = span.Source;
            StackTrace   = new StackTrace(2).ToString();
        }

        public static void ReportUnexpectedSyntax(TokenType expected, Span span) {
            var ex = new LangException(
                $"Invalid syntax, expected '{expected.GetValue()}', got '{span.Source.TokenStream.Peek.Type.GetValue()}'.",
                BlameSeverity.Error,
                span
            );
            ex.targetSource.Blames.Add(ex);
        }

        public static void Report(BlameType type, Span span) {
            var ex = new LangException(type.Description, type.Severity, span);
            ex.targetSource.Blames.Add(ex);
        }

        internal void PrintToConsole() {
            string[] codeLines =
                targetSource.TextStream.Text.Split(new[] { "\n" }, StringSplitOptions.None);

            var lines = new List<string>();
            // limit code piece by 5 lines
            for (int i = errorSpan.Start.Line; i < codeLines.Length && lines.Count < 4; i++) {
                lines.Add(codeLines[i].TrimEnd('\n', '\r', Spec.Eoc));
            }

            if (lines.Count > codeLines.Length - errorSpan.Start.Line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength = CliEditorSettings.LineNumberWidth + errorSpan.Start.Column;
            int errorTokenLength;
            if (errorSpan.End.Line > errorSpan.Start.Line) {
                errorTokenLength = lines[0].Length - errorSpan.Start.Column;
            }
            else {
                errorTokenLength = errorSpan.End.Column - errorSpan.Start.Column;
            }

            // underline, red-colored
            string pointer = new string(' ', pointerTailLength)
                           + new string('~', Math.Abs(errorTokenLength));

            //=========Error template=========
            //
            // Error: mismatching parenthesis.
            // --> C:\path\to\file.ax
            //
            // │   1 │ func("string",
            //             ~
            // │   2 │      'c',
            // │   3 │      123
            // ...
            //
            ConsoleColor color = Severity == BlameSeverity.Error
                ? ConsoleColor.Red
                : ConsoleColor.DarkYellow;

            // <severity>: <message>.
            ConsoleUI.WriteLine((Severity.ToString("G") + ": " + Message, color));
            // file name
            ConsoleUI.WriteLine(
                $"--> {targetSource.SourceFilePath}:{errorSpan.Start.Line + 1},{errorSpan.Start.Column + 1}");
            Console.WriteLine();
            // line with error
            CliEditor.DrawLineNumber(errorSpan.Start.Line + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, color));
            // next lines
            for (int i = errorSpan.Start.Line + 1; i < lines.Count; i++) {
                CliEditor.DrawLineNumber(i + 1);
                ConsoleUI.WriteLine(lines[i]);
            }

            Console.WriteLine();
        }

        public override string ToString() {
            return $"{Severity}: {Message} ({errorSpan.Start})";
        }
    }
}