using System;
using System.Diagnostics.Contracts;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class SymbolToken : Token {
        internal bool IsOpenBrace => Type == TokenType.LeftParenthesis
                                  || Type == TokenType.LeftBracket
                                  || Type == TokenType.LeftBrace;

        internal bool IsCloseBrace => Type == TokenType.RightParenthesis
                                   || Type == TokenType.RightBracket
                                   || Type == TokenType.RightBrace;

        [Pure]
        internal TokenType GetMatchingBrace() {
            switch (Type) {
                // open : close
                case TokenType.LeftParenthesis: return TokenType.RightParenthesis;
                case TokenType.LeftBracket:     return TokenType.RightBracket;
                case TokenType.LeftBrace:       return TokenType.RightBrace;
                // close : open
                case TokenType.RightParenthesis: return TokenType.LeftParenthesis;
                case TokenType.RightBracket:     return TokenType.LeftBracket;
                case TokenType.RightBrace:       return TokenType.LeftBrace;
                // should never be
                default: throw new NotSupportedException("Cannot return matching brace for non-brace operator.");
            }
        }

        public SymbolToken(Position startPosition, string value, string whitespaces = "")
            : base(Spec.Symbols[value], startPosition, value, whitespaces) {
        }
    }
}