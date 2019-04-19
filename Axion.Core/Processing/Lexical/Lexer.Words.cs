using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        /// <summary>
        ///     Gets a language keyword or identifier
        ///     from next piece of source.
        /// </summary>
        private Token? ReadWord() {
            do {
                tokenValue.Append(c);
                Move();
            } while (c.IsValidIdChar()
                     || c == '-'
                     && Peek.IsValidIdChar());

            if (Spec.Keywords.TryGetValue(tokenValue.ToString(), out TokenType kwType)) {
                if (tokens.Count > 0) {
                    Token last = tokens.Last();
                    if (last.Is(TokenType.KeywordIs) && kwType == TokenType.KeywordNot) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.Operators["is not"],
                            last.Span.StartPosition
                        );
                        return null;
                    }

                    if (last.Is(TokenType.KeywordNot) && kwType == TokenType.KeywordIn) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.Operators["not in"],
                            last.Span.StartPosition
                        );
                        return null;
                    }
                }

                // for 'and', 'or', 'is', etc.
                if (Spec.OperatorTypes.Select(x => x.type).Contains(kwType)) {
                    return new OperatorToken(tokenValue.ToString(), tokenStartPosition);
                }

                return new WordToken(kwType, tokenStartPosition);
            }

            return new WordToken(tokenValue.ToString(), tokenStartPosition);
        }
    }
}