using System.Linq;
using Axion.Core.Source;
using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class OperatorToken : Token {
        public int       Precedence { get; }
        public InputSide Side       { get; set; }

        internal OperatorToken(
            Unit      source,
            string    value     = "",
            TokenType tokenType = TokenType.None
        ) : base(source, tokenType, value) {
            if (tokenType != TokenType.None) {
                Value = Spec.Operators.First(kvp => kvp.Value.Item1 == tokenType).Key;
            }

            Content = Value;
            if (Spec.Operators.TryGetValue(Value, out (TokenType, int, InputSide) properties)) {
                (Type, Precedence, Side) = properties;
            }
            else {
                Precedence = -1;
                Side       = InputSide.Unknown;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum InputSide {
        Unknown,
        Both,
        Right,
        Left
    }
}
