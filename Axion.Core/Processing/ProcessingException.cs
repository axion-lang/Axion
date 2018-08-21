using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axion.Core.Tokens;
using Axion.Core.Visual;
using Axion.Core.Visual.ConsoleImpl;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    [JsonObject]
    public class ProcessingException : Exception {
        [JsonProperty] internal     string[] CodeLines;
        [JsonProperty] internal new string   Message;
        [JsonProperty] internal     string   Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        [JsonProperty] internal     int      ColumnPosition;
        [JsonProperty] internal     int      LinePosition;
        [JsonProperty] internal     Token    Token;

        internal ProcessingException(ErrorType type, string[] codeLines, Token errorToken) {
            CodeLines = codeLines;
            Message   = type.ToString("G"); // TODO add representations for <ErrorType>
            Token     = errorToken;
            // line positions need offset
            LinePosition   = Token.StartLinePos + 1;
            ColumnPosition = Token.StartColumnPos + 1;
        }

        internal ProcessingException(ErrorType type, string[] codeLines, LinkedListNode<Token> tokenNode) {
            CodeLines = codeLines;
            Message   = type.ToString("G"); // TODO add representations for <ErrorType>
            Token     = tokenNode.Value;
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
            // 8| variable ~~ "string"
            //             ^^
            // ...
            //
            // Error: MismatchingParenthesis.
            //
            // 1| func("string",
            // -------^
            //         'c',
            //         123
            // ...
            //
            // Write message
            ConsoleLog.Error(Message);

            var linesCount = 0;
            for (int i = Token.StartLinePos; i < CodeLines.Length && i < 4; i++) {
                linesCount++;
            }

            List<string> lines = CodeLines.Skip(Token.StartLinePos).Take(linesCount).ToList();
            // limit rest of code sign by 5 lines
            if (Token.StartLinePos + 5 < CodeLines.Length) {
                lines.Add("...");
            }
            //
            // FIRST LINE
            //
            // <line number>| <code line>
            int pointerTailLength = LinePosition.ToString().Length + 1 + ColumnPosition;
            // upside arrows (^), should be red-colored
            string pointer =
                // assemble tail of pointer
                new string('-', pointerTailLength) +
                // assemble pointer arrows
                new string('^', Token.Value.Length);

            // render line
            ConsoleUI.WriteLine($"{LinePosition}| {lines[0].TrimEnd(Spec.EndLine, Spec.EndStream)}");
            // render error pointer
            ConsoleUI.WriteLine((pointer, ConsoleColor.Red));
            //
            // NEXT LINES
            //
            for (int lineIndex = Token.StartLinePos + 1; lineIndex < lines.Count; lineIndex++) {
                // render next line
                ConsoleUI.WriteLine($"{lineIndex + 1}| {lines[lineIndex].TrimEnd(Spec.EndLine, Spec.EndStream)}");
            }
        }
    }
}