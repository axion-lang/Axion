using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    public partial class Lexer {
        /// <summary>
        ///     Gets a language keyword or identifier
        ///     from next piece of source.
        /// </summary>
        private WordToken ReadWord() {
            // don't use StringBuilder, language
            // words are mostly too short.
            string id = c.ToString();
            Move();
            while (c.IsValidIdChar()) {
                id += c;
                Move();
            }

            // return trailing restricted endings
            tokenValue.Append(id.TrimEnd(Spec.RestrictedIdentifierEndings));
            int explicitIdPartLength = id.Length - tokenValue.Length;
            if (explicitIdPartLength != 0) {
                Move(-explicitIdPartLength);
            }

            if (Spec.Keywords.TryGetValue(tokenValue.ToString(), out TokenType kwType)) {
                if (tokens.Count > 0) {
                    Token last = tokens.Last();
                    if (last.Is(TokenType.OpIs) && kwType == TokenType.OpNot) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.OperatorIsNot,
                            last.Span.StartPosition
                        );
                        return null;
                    }

                    if (last.Is(TokenType.OpNot) && kwType == TokenType.OpIn) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            Spec.OperatorNotIn,
                            last.Span.StartPosition
                        );
                        return null;
                    }
                }

                return new WordToken(kwType, tokenStartPosition);
            }

            return new WordToken(tokenValue.ToString(), tokenStartPosition);
        }
    }
}