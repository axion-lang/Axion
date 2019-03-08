using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

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
            string id = "" + c;
            stream.Move();
            while (Spec.IsValidIdChar(c)) {
                id += c;
                stream.Move();
            }

            // remove trailing restricted endings
            tokenValue.Append(id.TrimEnd(Spec.RestrictedIdentifierEndings));
            int explicitIdPartLength = id.Length - tokenValue.Length;
            if (explicitIdPartLength != 0) {
                stream.Move(-explicitIdPartLength);
            }

            if (Spec.Keywords.TryGetValue(tokenValue.ToString(), out TokenType kwType)) {
                if (tokens.Count > 0) {
                    Token last = tokens[tokens.Count - 1];
                    // is not
                    if (last.Type == TokenType.KeywordIs && kwType == TokenType.KeywordNot) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            last.Span.StartPosition,
                            Spec.OperatorIsNot
                        );
                        return null;
                    }
                    // not in
                    if (last.Type == TokenType.KeywordNot && kwType == TokenType.KeywordIn) {
                        tokens[tokens.Count - 1] = new OperatorToken(
                            last.Span.StartPosition,
                            Spec.OperatorNotIn
                        );
                        return null;
                    }
                }
                return new KeywordToken(kwType, tokenStartPosition);
            }
            return new IdentifierToken(tokenStartPosition, tokenValue.ToString());
        }
    }
}