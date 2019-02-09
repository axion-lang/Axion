using System.Linq;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'keyword'.
    /// </summary>
    public class KeywordToken : Token {
        public KeywordToken(TokenType tokenType, Position startPosition, string whitespaces = "") : base(
            tokenType,
            startPosition,
            Spec.Keywords.First(kvp => kvp.Value == tokenType).Key,
            whitespaces
        ) {
        }

        /// <summary>
        ///     Constructor for compound keywords.
        /// </summary>
        public KeywordToken(TokenType tokenType, Position startPosition, string value, string whitespaces = "") : base(
            tokenType,
            startPosition,
            value,
            whitespaces
        ) {
        }
    }
}