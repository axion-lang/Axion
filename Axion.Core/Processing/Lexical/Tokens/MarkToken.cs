using System.Linq;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'symbolic mark' literal.
    /// </summary>
    public class MarkToken : Token {
        public MarkToken(string value, Position startPosition = default)
            : base(
                Spec.Symbols[value],
                value,
                startPosition
            ) { }

        public MarkToken(TokenType type)
            : base(
                type,
                Spec.Symbols.First(kvp => kvp.Value == type).Key
            ) { }
    }
}