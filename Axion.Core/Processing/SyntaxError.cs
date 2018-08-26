using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axion.Core.Tokens;
using Axion.Core.Visual;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    [JsonObject]
    public class SyntaxError : Exception {
        [JsonProperty] internal     string Code;
        [JsonProperty] internal new string Message;
        [JsonProperty] internal     string Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        [JsonProperty] internal     int    ColumnPosition;
        [JsonProperty] internal     int    LinePosition;
        [JsonProperty] internal     Token  Token;

        internal SyntaxError(ErrorType type, string code, Token errorToken)
            : base(type.ToString("G")) {
            Code    = code;
            Message = type.ToString("G"); // TODO add representations for <ErrorType>
            Token   = errorToken;
            // line positions need offset
            LinePosition   = Token.StartLinePos + 1;
            ColumnPosition = Token.StartColumnPos + 1;
        }

        /// <summary>
        ///     Renders visual representation of occurred error.
        /// </summary>
        internal void Render() {
            //--------Error templates--------
            //
            // Error: Invalid operator.
            //
            // |   8 | variable ~~ "string"
            //             ^^
            // ...
            //
            // Error: MismatchingParenthesis.
            //
            // |   1 | func("string",
            // -------------^
            // |   2 |      'c',
            // |   3 |      123
            // ...
            //
            // Write message
            ConsoleLog.Error(Message);
            ConsoleUI.WriteLine(
                ("At line " + (Token.StartLinePos + 1) + ", column " + (Token.StartColumnPos + 1) + ".",
                 ConsoleColor.Red)
            );

            string[] codeLines = Code.Split(Spec.Newlines, StringSplitOptions.None);

            var linesCount = 0;
            for (int i = Token.StartLinePos; i < codeLines.Length && i < 4; i++) {
                linesCount++;
            }

            List<string> lines = codeLines.Skip(Token.StartLinePos).Take(linesCount).ToList();
            // limit rest of code sign by 5 lines
            if (Token.StartLinePos + 5 < codeLines.Length) {
                lines.Add("...");
            }
            // first line
            // <line number>| <code line>
            int pointerTailLength = ConsoleCodeEditor.LineNumberWidth + ColumnPosition;
            // upside arrows (^), should be red-colored
            string pointer =
                // assemble tail of pointer
                new string('-', pointerTailLength) +
                // assemble pointer arrows
                new string(
                    '^',
                    Math.Min(
                        Token.Value.Length, ConsoleCodeEditor.LineNumberWidth + lines[0].Length - pointerTailLength
                    )
                );

            // render line
            ConsoleCodeEditor.PrintLineNumber(LinePosition);
            ConsoleUI.WriteLine(lines[0].TrimEnd(Spec.EndLine, Spec.EndStream));
            // render error pointer
            ConsoleUI.WriteLine((pointer, ConsoleColor.Red));

            // render some next lines
            for (int lineIndex = Token.StartLinePos + 1; lineIndex < lines.Count; lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex].TrimEnd(Spec.EndLine, Spec.EndStream));
            }
        }
    }
}