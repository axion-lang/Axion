using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing;
using Axion.Core.Tokens;
using Axion.Core.Visual.ConsoleImpl;

namespace Axion.Core.Visual {
    internal class ConsoleSyntaxHighlighter {
        public void Highlight(string inputCodeLine, out List<ProcessingException> errors) {
            var lexer = new Lexer(
                new[] { inputCodeLine },
                out LinkedList<Token> tokens,
                out errors,
                SourceProcessingOptions.PreserveWhitespaces
            );
            lexer.Process();
            for (var i = 0; i < tokens.Count; i++) {
                Token token = tokens.ElementAt(i);
                if (token is OneLineCommentToken oneLineComment) {
                    ConsoleUI.Write((oneLineComment.ToAxionCode(), ConsoleColor.DarkGreen));
                }
                else if (token is MultilineCommentToken multilineComment) {
                    ConsoleUI.Write((multilineComment.ToAxionCode(), ConsoleColor.DarkGreen));
                }
                else if (token is NumberToken number) {
                    ConsoleUI.Write((number.ToAxionCode(), ConsoleColor.Yellow));
                }
                else if (token is StringToken stringToken) {
                    ConsoleUI.Write((stringToken.ToAxionCode(), ConsoleColor.DarkYellow));
                }
                else if (token is OperatorToken operatorToken) {
                    ConsoleUI.Write((operatorToken.ToAxionCode(), ConsoleColor.Red));
                }
                else if (token is KeywordToken keyword) {
                    ConsoleUI.Write((keyword.ToAxionCode(), ConsoleColor.DarkCyan));
                }
                else if (token is EndOfStreamToken) {
                    break;
                }
                else {
                    ConsoleUI.Write(token.Value);
                }
            }
        }
    }
}