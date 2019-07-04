using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Axion.Core.Processing.Source;
using Axion.Core.Specification;
using CodeConsole;
using CodeConsole.CodeEditor;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    public class LanguageException : Exception {
        public override string Message { get; }
        public override string StackTrace { get; }
        public BlameSeverity Severity { get; }
        public Span Span { get; }

        [JsonProperty] private readonly string time = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        public LanguageException(
            BlameType     type,
            BlameSeverity severity,
            Position      start,
            Position      end
        ) {
            Severity   = severity;
            Span       = new Span(start, end);
            Message    = TypeToMessage(type);
            StackTrace = new StackTrace(2).ToString();
        }

        public LanguageException(string message, BlameSeverity severity, Span span) {
            Severity   = severity;
            Span       = span;
            Message    = message;
            StackTrace = new StackTrace(2).ToString();
        }

        /// <summary>
        ///     Creates visual representation of occurred error in console.
        /// </summary>
        internal void Print(SourceUnit unit) {
            //--------Error templates--------
            //
            // Error: Invalid operator.
            //
            // |   8 | variable ~~ "string"
            // -----------------^^
            // ...
            //
            // Error: Mismatching parenthesis.

            // |   1 | func("string",
            // ------------^
            // |   2 |      'c',
            // |   3 |      123
            // ...
            //

            ConsoleColor color = Severity == BlameSeverity.Error
                ? ConsoleColor.Red
                : ConsoleColor.DarkYellow;

            // Write message
            ConsoleUI.WriteLine((Message, color));
            // Append file name if it's exists.
            ConsoleUI.WriteLine("In file '" + unit.SourceFilePath + "'.");

            Console.WriteLine();

            string[] codeLines = unit.Code.Split(Spec.EndOfLines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = Span.Start.Line;
                 i < codeLines.Length && lines.Count < 4;
                 i++) {
                lines.Add(codeLines[i].TrimEnd('\n', Spec.EndOfCode));
            }

            if (lines.Count > codeLines.Length - Span.Start.Line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength =
                CliEditorSettings.LineNumberWidth + Span.Start.Column;
            int errorTokenLength;
            if (Span.End.Line > Span.Start.Line) {
                errorTokenLength = lines[0].Length - Span.Start.Column;
            }
            else {
                errorTokenLength = Span.End.Column - Span.Start.Column;
            }

            // upside arrows (^), should be red-colored
            string pointer =
                // tail of pointer
                new string(' ', pointerTailLength)
               +
                // pointer arrows
                new string(
                    '^', // TODO (UI) compute token value length: include tab lengths
                    errorTokenLength
                );

            // Drawing ==========

            // line with error
            CliEditor.DrawLineNumber(Span.Start.Line + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, color));

            // next lines
            for (int i = Span.Start.Line + 1; i < lines.Count; i++) {
                CliEditor.DrawLineNumber(i + 1);
                ConsoleUI.WriteLine(lines[i]);
            }

            Console.WriteLine();
        }

        private static string TypeToMessage(BlameType type) {
            string enumMemberName = type.ToString("G");

            var result = new StringBuilder();
            result.Append(char.ToUpper(enumMemberName[0]));

            enumMemberName = enumMemberName.Remove(0, 1);
            foreach (char c in enumMemberName) {
                if (char.IsUpper(c)) {
                    result.Append(" ").Append(char.ToLower(c));
                }
                else {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public override string ToString() {
            return $"{Severity}: {Message} ({Span.Start})";
        }
    }
}