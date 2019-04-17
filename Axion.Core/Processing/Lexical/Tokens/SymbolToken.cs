using System.Linq;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'symbol' token.
    /// </summary>
    public class SymbolToken : Token {
        public SymbolToken(string value, Position startPosition = default)
            : base(
                Spec.Symbols[value],
                value,
                startPosition
            ) { }

        public SymbolToken(TokenType type)
            : base(
                type,
                Spec.Symbols.First(kvp => kvp.Value == type).Key
            ) { }
    }
}