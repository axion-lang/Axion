using System;
using System.Collections.Generic;
using System.Globalization;
using AxionStandard.Enums;
using AxionStandard.Processing.Tokens;
using Newtonsoft.Json;

namespace AxionStandard.Processing {
   [JsonObject]
   internal class ProcessingException : Exception {
      [JsonProperty] public new string Message;
      [JsonProperty] public string Time;
      [JsonProperty] public IEnumerable<Token> TracebackList;

      internal ProcessingException(string errorMessage, ErrorOrigin errorOrigin,
                                   int lnPos = -1, int clPos = -1) {
         // line positions need offset
         lnPos++;
         clPos++;
         Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);
         Message = $"{errorOrigin:G} error: {errorMessage}.";
         if (lnPos != 0) {
            Message += $"\r\nAt line {lnPos}";
            if (clPos != 0) {
               Message += $", column {clPos}";
            }
            Message += ".";
         }

         switch (errorOrigin) {
            case ErrorOrigin.Lexer: {
               TracebackList = Compiler.ProcessingSource.Tokens;
               break;
            }
            case ErrorOrigin.Parser: {
               TracebackList = Compiler.ProcessingSource.SyntaxTree;
               break;
            }
         }

         Logger.LogInfo("Saving full traceback to file ...");
         Compiler.ProcessingSource.SaveDebugInfoToFile();

         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(JsonConvert.SerializeObject(this, Compiler.JsonSerializerSettings));
         Console.WriteLine();
         Console.ForegroundColor = ConsoleColor.White;
         Console.Write(Compiler.AnyKeyToClose);
      }
   }
}