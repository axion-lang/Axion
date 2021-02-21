using System.Linq;
using Axion.Core.Hierarchy;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class OperatorToken : Token {
        public int       Precedence { get; }
        public InputSide Side       { get; set; }

        internal OperatorToken(
            Unit      unit,
            string    value     = "",
            TokenType tokenType = TokenType.None
        ) : base(unit, tokenType, value) {
            if (tokenType != TokenType.None) {
                Value = Spec.Operators
                            .First(o => o.Value.Type == tokenType)
                            .Key;
            }

            Content = Value;
            if (Spec.Operators.TryGetValue(
                Value,
                out var properties
            )) {
                (Type, Precedence, Side) = properties;
            }
            else {
                Precedence = -1;
                Side       = InputSide.Unknown;
            }
        }
    }
}
