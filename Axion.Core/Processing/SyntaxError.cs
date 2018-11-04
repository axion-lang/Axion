using System;
using System.Collections.Generic;
using System.Globalization;
using Axion.Core.Tokens;
using Axion.Core.Visual;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    [JsonObject]
    public class SyntaxError : Exception {
        [JsonProperty] internal string Code;

        [JsonProperty] internal SourceCode File;

        [JsonProperty] internal new string Message;

        [JsonProperty] internal string Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        [JsonProperty] internal ErrorType Type;

        [JsonProperty] internal Token Token;

        internal SyntaxError(ErrorType type, string code, Token errorToken)
            : base(type.ToString("G")) {
            Type    = type;
            Code    = code;
            Message = type.ToString("G"); // TODO add strings for <ErrorType>
            Token   = errorToken;
            // line positions need offset
        }

        internal SyntaxError(ErrorType type, SourceCode file, Token errorToken)
            : base(type.ToString("G")) {
            Type    = type;
            File    = file;
            Code    = file.Code;
            Message = type.ToString("G"); // TODO add strings for <ErrorType>
            Token   = errorToken;
            // line positions need offset
        }

        internal SyntaxError(WarningType type, string code, Token errorToken)
            : base(type.ToString("G")) {
            Code    = code;
            Message = type.ToString("G"); // TODO add strings for <ErrorType>
            Token   = errorToken;
            // line positions need offset
        }

        internal SyntaxError(WarningType type, SourceCode file, Token errorToken)
            : base(type.ToString("G")) {
            File    = file;
            Code    = file.Code;
            Message = type.ToString("G"); // TODO add strings for <ErrorType>
            Token   = errorToken;
            // line positions need offset
        }

        /// <summary>
        ///     Creates visual representation of occurred error.
        /// </summary>
        internal void Draw() {
            //--------Error templates--------
            //
            // Error: Invalid operator.
            //
            // |   8 | variable ~~ "string"
            // -----------------^^
            // ...
            //
            // Error: MismatchingParenthesis.
            //
            // |   1 | func("string",
            //             ^
            // |   2 |      'c',
            // |   3 |      123
            // ...
            //
            // Write message
            ConsoleUI.LogError(Message);
            if (File != null) {
                ConsoleUI.WriteLine("In file '" + File.SourceFilePath + "'.");
            }
            ConsoleUI.WriteLine(("At line " + (Token.StartLine + 1) + ", column " + (Token.StartColumn + 1) + ".", ConsoleColor.Red));

            string[] codeLines = Code.Split(Spec.Newlines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = Token.StartLine; i < codeLines.Length && lines.Count < 4; i++) {
                lines.Add(codeLines[i]);
            }
            if (lines.Count > codeLines.Length - Token.StartLine) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength = ConsoleCodeEditor.LineNumberWidth + Token.StartColumn + 1;
            int errorTokenLength  = Token.EndColumn - Token.StartColumn;
            if (errorTokenLength > 1) {
                errorTokenLength++;
            }
            // upside arrows (^), should be red-colored
            string pointer =
                // tail of pointer
                new string(' ', pointerTailLength) +
                // pointer arrows
                new string(
                    '^', // TODO compute token value length: include tab lengths
                    Math.Min(
                        errorTokenLength, ConsoleCodeEditor.LineNumberWidth + lines[0].Length - pointerTailLength
                    ) // BUG: pointerTailLength: fails sometimes
                );

            // Drawing --------------------------------------------------------------------------

            // line with error
            ConsoleCodeEditor.PrintLineNumber(Token.StartLine + 1);
            ConsoleUI.WriteLine(lines[0].TrimEnd(Spec.EndOfLine, Spec.EndOfStream));
            // error pointer
            ConsoleUI.WriteLine((pointer, ConsoleColor.Red));

            // next lines
            for (int lineIndex = Token.StartLine + 1; lineIndex < lines.Count; lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex].TrimEnd(Spec.EndOfLine, Spec.EndOfStream));
            }
        }
    }
}