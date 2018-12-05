using System.Linq;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;language keyword&gt; <see cref="Token" />.
    /// </summary>
    public class KeywordToken : Token {
        public KeywordToken(TokenType tokenType, (int, int) startPosition, string whitespaces = "")
            : base(tokenType, startPosition, Spec.Keywords.First(kvp => kvp.Value == tokenType).Key, whitespaces) {
        }
    }
}