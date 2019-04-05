using System.Linq;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'identifier' or 'keyword'.
    /// </summary>
    public class WordToken : Token {
        /// <summary>
        ///     Constructor for keywords creation.
        /// </summary>
        public WordToken(TokenType tokenType, Position startPosition = default) :
            base(
                tokenType,
                Keywords.First(kvp => kvp.Value == tokenType).Key,
                startPosition
            ) { }

        public WordToken(string value, Position startPosition = default) :
            base(TokenType.Identifier, value, startPosition) {
            if (Keywords.TryGetValue(Value, out TokenType kwType)) {
                Type = kwType;
            }
        }
    }
}