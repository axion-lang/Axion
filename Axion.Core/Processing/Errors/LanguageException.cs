using System;
using System.Collections.Generic;
using System.Globalization;
using Axion.Core.Specification;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    [JsonObject]
    [Serializable]
    public class LanguageException : Exception {
        [JsonProperty]
        internal new string Message { get; }

        [JsonProperty]
        private SourceUnit src { get; }

        [JsonProperty]
        private string code { get; }

        [JsonProperty]
        private string time { get; } = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        [JsonProperty]
        private Blame blame { get; }

        internal LanguageException(Blame blame, string code)
            : base(blame.AsMessage()) {
            this.blame = blame;
            this.code  = code;
            Message    = base.Message;
        }

        internal LanguageException(Blame blame, SourceUnit source)
            : this(blame, source.Code) {
            src = source;
        }

        /// <summary>
        ///     Creates visual representation of occurred error in console.
        /// </summary>
        internal void Print() {
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
            //             ^
            // |   2 |      'c',
            // |   3 |      123
            // ...
            //

            ConsoleColor color = blame.Severity == BlameSeverity.Error
                                     ? ConsoleColor.Red
                                     : ConsoleColor.DarkYellow;

            // Write message
            ConsoleUI.WriteLine((Message, color));
            // Append file name if it's exists.
            if (src != null) {
                ConsoleUI.WriteLine("In file '" + src.SourceFilePath + "'.");
            }
            Console.WriteLine();

            string[] codeLines = code.Split(Spec.EndOfLines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = blame.Span.StartPosition.Line; i < codeLines.Length && lines.Count < 4; i++) {
                lines.Add(codeLines[i].TrimEnd('\n', Spec.EndOfStream));
            }
            if (lines.Count > codeLines.Length - blame.Span.StartPosition.Line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength = ConsoleCodeEditor.LineNumberWidth + blame.Span.StartPosition.Column;
            int errorTokenLength;
            if (blame.Span.EndPosition.Line > blame.Span.StartPosition.Line) {
                // token multiline
                errorTokenLength = lines[0].Length - blame.Span.StartPosition.Column;
            }
            else {
                errorTokenLength = blame.Span.EndPosition.Column - blame.Span.StartPosition.Column;
            }
            // upside arrows (^), should be red-colored
            string pointer =
                // tail of pointer
                new string(' ', pointerTailLength) +
                // pointer arrows
                new string(
                    '^', // TODO compute token value length: include tab lengths
                    errorTokenLength
                );

            // Drawing --------------------------------------------------------------------------

            // line with error
            ConsoleCodeEditor.PrintLineNumber(blame.Span.StartPosition.Line + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, color));

            // next lines
            for (int lineIndex = blame.Span.StartPosition.Line + 1; lineIndex < lines.Count; lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex]);
            }
            Console.WriteLine();
        }
    }
}