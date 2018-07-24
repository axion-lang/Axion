using System;
using System.Globalization;
using System.Linq;
using Axion.Tokens;
using Newtonsoft.Json;

namespace Axion.Processing {
    [JsonObject]
    public class ProcessingException : Exception {
        [JsonProperty] internal     int    ColumnPosition;
        [JsonProperty] internal     int    LinePosition;
        [JsonProperty] internal new string Message;
        [JsonProperty] internal     string Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        [JsonProperty] internal     Token  Token;

        internal ProcessingException(Exception inner, ErrorType type, string sourceLine, Token errorToken) {
        }

        internal ProcessingException(ErrorType type, SourceCode source, Token errorToken) {
            /* -----Error template-----
             *
             * Error: Invalid operator.
             *
             * 8| variable ~~ "string"
             *             ^^
             *
             */
            // TODO add string representations for <ErrorType>
            Message = type.ToString("G");
            Token   = errorToken;
            // line positions need offset
            LinePosition   = errorToken.StartLnPos + 1;
            ColumnPosition = errorToken.StartClPos + 1;
            string sourceLine         = source.Content[Token.StartLnPos];
            var    pointer            = ""; // upside arrows (^), should be red-colored
            int    sourceIndentLength = sourceLine.TakeWhile(char.IsWhiteSpace).Count();
            // render space before pointer
            int pointerPosition =
                LinePosition.ToString().Length + 1 + ColumnPosition - sourceIndentLength;
            for (var i = 0; i < pointerPosition; i++) {
                pointer += " ";
            }
            // render pointer arrows
            for (var i = 0; i < errorToken.Value.Length; i++) {
                pointer += "^";
            }

            // Write message
            Log.Write("Error: ", ConsoleColor.Red);
            Console.WriteLine(Message + ".");
            Console.WriteLine($"{LinePosition}| {sourceLine.Trim()}");
            // Write pointer
            Log.WriteLine(pointer, ConsoleColor.Red);
            // Traceback
            Log.Info("Saving full traceback to file ...");
            source.SaveDebugInfoToFile();
            //Console.WriteLine(JsonConvert.SerializeObject(this, Compiler.JsonSerializerSettings));
            Console.Write("Press any key to close app.");
            Console.WriteLine();
        }
    }
}