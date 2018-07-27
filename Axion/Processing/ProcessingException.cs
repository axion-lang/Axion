using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axion.Tokens;
using Newtonsoft.Json;

namespace Axion.Processing {
    [JsonObject]
    public class ProcessingException : Exception {
        [JsonProperty] internal new SourceCode Source;
        [JsonProperty] internal     int        ColumnPosition;
        [JsonProperty] internal     int        LinePosition;
        [JsonProperty] internal new string     Message;
        [JsonProperty] internal     string     Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        [JsonProperty] internal     Token      TokenPrev2;
        [JsonProperty] internal     Token      TokenPrev1;
        [JsonProperty] internal     Token      Token;
        [JsonProperty] internal     Token      TokenNext1;
        [JsonProperty] internal     Token      TokenNext2;

        internal ProcessingException(ErrorType type, SourceCode source, Token errorToken) {
            Source  = source;
            Message = type.ToString("G"); // TODO add representations for <ErrorType>
            Token   = errorToken;
            // line positions need offset
            LinePosition   = Token.StartLnPos + 1;
            ColumnPosition = Token.StartClPos + 1;
        }

        internal ProcessingException(ErrorType type, SourceCode source, LinkedListNode<Token> tokenNode) {
            Source     = source;
            Message    = type.ToString("G"); // TODO add representations for <ErrorType>
            Token      = tokenNode.Value;
            TokenPrev1 = tokenNode.Previous?.Value;
            TokenPrev2 = tokenNode.Previous?.Previous?.Value;
            TokenNext1 = tokenNode.Next?.Value;
            TokenNext2 = tokenNode.Next?.Next?.Value;
            // line positions need offset
            LinePosition   = Token.StartLnPos + 1;
            ColumnPosition = Token.StartClPos + 1;
        }

        /// <summary>
        ///     Renders visual representation of occurred error.
        /// </summary>
        internal void Render() {
            /* -----Error template-----
             *
             * Error: Invalid operator.
             *
             * 8| variable ~~ "string"
             *             ^^
             *
             *
             *
             *
             * Error: MismatchingParenthesis.
             *
             * 1| func("string",
             * -------^
             *         'c',
             *         123
             *
             */
            // Write message
            Log.Write("Error: ", ConsoleColor.Red);
            Console.WriteLine(Message + ".");

            var linesCount = Token.EndLnPos - Token.StartLnPos;
            if (Token.EndLnPos + 1 < Source.Lines.Length) {
                linesCount++;
            }
            var pointer = ""; // upside arrows (^), should be red-colored

            List<string> lines = Source.Lines.Skip(Token.StartLnPos).Take(linesCount).ToList();
            foreach (string line in lines) {
                // <number>| <code>
                int pointerPosition =
                    LinePosition.ToString().Length + 2 + ColumnPosition;
                // render space before pointer
                for (var i = 0; i < pointerPosition; i++) {
                    pointer += " ";
                }
                // render pointer arrows
                for (var i = 0; i < Token.Value.Length; i++) {
                    pointer += "^";
                }
                Console.WriteLine($"{LinePosition}| {line.Trim()}");
                // Write pointer
                Log.WriteLine(pointer, ConsoleColor.Red);
                pointer = "";
            }

            // Traceback
            Log.Info("Saving full traceback to file ...");
            Source.SaveDebugInfoToFile();
            //Console.WriteLine(JsonConvert.SerializeObject(this, Compiler.JsonSerializerSettings));
            Console.Write("Press any key to close app.");
            Console.WriteLine();
        }
    }
}