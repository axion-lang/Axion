using System;
using System.Collections.Generic;
using System.Globalization;
using Axion.Core.Tokens;
using ConsoleExtensions;
using Newtonsoft.Json;

namespace Axion.Core.Processing {
    [JsonObject]
    [Serializable]
    public class SyntaxException : Exception {
        [JsonProperty] internal string Code;

        [JsonProperty] internal SourceCode File;

        [JsonProperty] internal new string Message;

        [JsonProperty] internal string Time = DateTime.Now.ToString(CultureInfo.InvariantCulture);

        [JsonProperty] internal Token Token;

        internal SyntaxException(ErrorType type, string code, Token errorToken)
            : base(type.ToString("G") + $" ({errorToken?.StartLine + 1}, {errorToken?.StartColumn + 1})") {
            Code    = code;
            Token   = errorToken;
            Message = base.Message;
            // TODO add informative descriptions for <ErrorType>
        }

        internal SyntaxException(ErrorType type, SourceCode file, Token errorToken)
            : this(type, file.Code, errorToken) {
            Code = file.Code;
        }

        internal SyntaxException(WarningType type, string code, Token errorToken)
            : base(type.ToString("G") + $" ({errorToken.StartLine + 1}, {errorToken.StartColumn + 1})") {
            Code    = code;
            Token   = errorToken;
            Message = base.Message;
        }

        internal SyntaxException(WarningType type, SourceCode file, Token errorToken)
            : this(type, file.Code, errorToken) {
            File = file;
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

            string[] codeLines = Code.Split(Spec.EndOfLines, StringSplitOptions.None);

            var lines = new List<string>();
            // limit rest of code by 5 lines
            for (int i = Token.StartLine; i < codeLines.Length && lines.Count < 4; i++) {
                lines.Add(codeLines[i].TrimEnd('\n', Spec.EndOfStream));
            }
            if (lines.Count > codeLines.Length - Token.StartLine) {
                lines.Add("...");
            }

            // first line
            // <line number>| <code line>
            int pointerTailLength = ConsoleCodeEditor.LineNumberWidth + Token.StartColumn;
            int errorTokenLength;
            if (Token.EndColumn < Token.StartColumn) {
                // token multiline
                errorTokenLength = lines[0].Length - Token.StartColumn;
            }
            else {
                errorTokenLength = Token.EndColumn - Token.StartColumn;
            }
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
                    errorTokenLength
                );

            // Drawing --------------------------------------------------------------------------

            // line with error
            ConsoleCodeEditor.PrintLineNumber(Token.StartLine + 1);
            ConsoleUI.WriteLine(lines[0]);
            // error pointer
            ConsoleUI.WriteLine((pointer, ConsoleColor.Red));

            // next lines
            for (int lineIndex = Token.StartLine + 1; lineIndex < lines.Count; lineIndex++) {
                ConsoleCodeEditor.PrintLineNumber(lineIndex + 1);
                ConsoleUI.WriteLine(lines[lineIndex]);
            }
        }
    }
}