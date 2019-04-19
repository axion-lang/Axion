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

            string value = tokenValue.ToString();
            // for operators, written as words
            if (Spec.Operators.TryGetValue(value, out OperatorProperties props)) {
                if (tokens.Count > 0) {
                    Token last = tokens[tokens.Count - 1];
                    if (last.Is(TokenType.OpIs) && props.Type == TokenType.OpNot) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.Operators["is not"],
                            last.Span.StartPosition,
                            Position
                        );
                        return null;
                    }

                    if (last.Is(TokenType.OpNot) && props.Type == TokenType.OpIn) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.Operators["not in"],
                            last.Span.StartPosition,
                            Position
                        );
                        return null;
                    }
                }

                return new OperatorToken(value, tokenStartPosition);
            }

            return new WordToken(value, tokenStartPosition);
        }
    }
}