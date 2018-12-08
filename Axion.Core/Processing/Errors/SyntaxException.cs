using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    [JsonObject]
    [Serializable]
    public class SyntaxException : Exception {
        [JsonProperty]
        internal new string Message { get; }

        [JsonProperty]
        private SourceCode file { get; }

        [JsonProperty]
        private string code { get; }

        [JsonProperty]
        private string time { get; } = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        [JsonProperty]
        private IBlame blame { get; }

        internal SyntaxException(IBlame blame, string code)
            : base(blame.AsMessage()) {
            this.blame = blame;
            this.code  = code;
            Message    = base.Message;
        }

        internal SyntaxException(IBlame warning, SourceCode file)
            : this(warning, file.Code) {
            this.file = file;
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

            // Write message
            ConsoleUI.WriteLine(
                (
                    Message,
                    blame is Error
                        ? ConsoleColor.Red
                        : ConsoleColor.DarkYellow
                )
            );
            // Append file name if it's exists.
            if (file != null) {
                ConsoleUI.WriteLine("In file '" + file.SourceFilePath + "'.");
            }
            Console.WriteLine();

            string[] codeLines = code.Split(Spec.EndOfLines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = blame.StartPosition.line; i < codeLines.Length && lines.Count < 4; i++) {
                lines.Add(codeLines[i].TrimEnd('\n', Spec.EndOfStream));
            }
            if (lines.Count > codeLines.Length - blame.StartPosition.line) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength = ConsoleCodeEditor.LineNumberWidth + blame.StartPosition.column;
            int errorTokenLength;
            if (blame.EndPosition.column < blame.StartPosition.column) {
                // token multiline
                errorTokenLength = lines[0].Length - blame.StartPosition.column;
            }
            else {
                errorTokenLength = blame.EndPosition.column - blame.StartPosition.column;
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
            ConsoleCodeEditor.PrintLineNumber(blame.StartPosition.line + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, ConsoleColor.Red));

            // next lines
            for (int lineIndex = blame.StartPosition.line + 1; lineIndex < lines.Count; lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex]);
            }
            Console.WriteLine();
        }
    }
}