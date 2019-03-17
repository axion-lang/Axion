using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Lexer {
    public partial class Lexer {
        /// <summary>
        ///     Gets a language keyword or identifier from next piece of source.
        ///     Returns keyword token if next piece of source
        ///     is keyword, declared in specification.
        ///     Otherwise, returns identifier token.
        /// </summary>
        private Token ReadWord() {
            // don't use StringBuilder, language
            // words are mostly too short.
            string id = c.ToString();
            stream.Move();
            while (IsValidIdChar(c)) {
                id += c;
                stream.Move();
            }

            // return trailing restricted endings
            tokenValue.Append(id.TrimEnd(RestrictedIdentifierEndings));
            int explicitIdPartLength = id.Length - tokenValue.Length;
            if (explicitIdPartLength != 0) {
                stream.Move(-explicitIdPartLength);
            }

            if (Keywords.TryGetValue(tokenValue.ToString(), out TokenType kwType)) {
                if (tokens.Count > 0) {
                    Token last = tokens[tokens.Count - 1];
                    if (last.Is(TokenType.OpIs)
                        && kwType == TokenType.OpNot) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            OperatorIsNot,
                            last.Span.StartPosition
                        );
                        return null;
                    }

                    if (last.Is(TokenType.OpNot)
                        && kwType == TokenType.OpIn) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            OperatorNotIn,
                            last.Span.StartPosition
                        );
                        return null;
                    }
                }

                return new KeywordToken(kwType, tokenStartPosition);
            }

            return new IdentifierToken(tokenValue.ToString(), tokenStartPosition);
        }
    }
}