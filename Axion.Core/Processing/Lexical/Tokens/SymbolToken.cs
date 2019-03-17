using System;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'symbol' literal.
    /// </summary>
    public class SymbolToken : Token {
        public SymbolToken(string value, Position startPosition = default) : base(
            Spec.Symbols.Forward[value],
            value,
            startPosition
        ) { }

        public SymbolToken(TokenType type) : base(
            type,
            Spec.Symbols.Reverse[type]
        ) { }

        internal bool IsOpenBrace =>
            Is(TokenType.LeftParenthesis)
            || Is(TokenType.LeftBracket)
            || Is(TokenType.LeftBrace);

        internal bool IsCloseBrace =>
            Is(TokenType.RightParenthesis)
            || Is(TokenType.RightBracket)
            || Is(TokenType.RightBrace);

        internal TokenType GetMatchingBrace() {
            switch (Type) {
                // open : close
                case TokenType.LeftParenthesis:
                    return TokenType.RightParenthesis;
                case TokenType.LeftBracket:
                    return TokenType.RightBracket;
                case TokenType.LeftBrace:
                    return TokenType.RightBrace;
                // close : open
                case TokenType.RightParenthesis:
                    return TokenType.LeftParenthesis;
                case TokenType.RightBracket:
                    return TokenType.LeftBracket;
                case TokenType.RightBrace:
                    return TokenType.LeftBrace;
                // should never be
                default:
                    throw new NotSupportedException(
                        "Cannot return matching brace for non-brace operator."
                    );
            }
        }
    }
}