using System.Linq;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;language keyword&gt; <see cref="Token" />.
    /// </summary>
    public class KeywordToken : Token {
        private readonly KeywordType KeywordType;

        public KeywordToken(KeywordType keywordType, (int, int) startPosition, string whitespaces = "")
            : base(TokenType.Keyword, startPosition, Spec.Keywords.First(x => x.Value == keywordType).Key, whitespaces) {
            KeywordType = keywordType;
        }
    }
}