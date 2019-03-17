using System.Linq;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'keyword'.
    /// </summary>
    public class KeywordToken : Token {
        public KeywordToken(TokenType tokenType, Position startPosition = default) :
            base(
                tokenType,
                Spec.Keywords.First(kvp => kvp.Value == tokenType).Key,
                startPosition
            ) { }
    }
}