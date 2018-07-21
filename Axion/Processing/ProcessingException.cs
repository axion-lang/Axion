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
            Message = type.ToString("G");
            Token   = errorToken;
            // line positions need offset
            LinePosition   = errorToken.StartLnPos + 1;
            ColumnPosition = errorToken.StartClPos + 1;
            /*  ----Error template----
             * Error: Invalid operator.
             *
             * 8| variable ~~ "string"
             *             ^^
             */
            var sourceLine         = source.Content[Token.StartLnPos];
            var pointer            = ""; // upside arrows (^), should be red-colored
            var sourceIndentLength = sourceLine.TakeWhile(char.IsWhiteSpace).Count();
            // render space before pointer
            var pointerPosition =
                LinePosition.ToString().Length + 1 + ColumnPosition - sourceIndentLength;
            for (var i = 0; i < pointerPosition; i++) {
                pointer += " ";
            }
            // render pointer arrows
            for (var i = 0; i < errorToken.Value.Length; i++) {
                pointer += "^";
            }

            // Write message
            Logger.Colored("Error: ", ConsoleColor.Red);
            Console.WriteLine(Message + ".");
            Console.WriteLine($"{LinePosition}| {sourceLine.Trim()}");
            // Write pointer
            Logger.ColoredLine(pointer, ConsoleColor.Red);
            // Traceback
            Logger.Info("Saving full traceback to file ...");
            source.SaveDebugInfoToFile();
            //Console.WriteLine(JsonConvert.SerializeObject(this, Compiler.JsonSerializerSettings));
            Console.Write("Press any key to close app.");
            Console.WriteLine();
        }
    }
}