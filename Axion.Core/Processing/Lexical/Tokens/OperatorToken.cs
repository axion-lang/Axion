using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'operator' literal.
    /// </summary>
    public class OperatorToken : Token {
        internal OperatorProperties Properties;

        public OperatorToken(
            OperatorProperties properties,
            Position           startPosition = default
        ) : base(
            properties.Type,
            startPosition: startPosition
        ) {
            Properties = properties;
        }

        public OperatorToken(string value, Position startPosition = default) : base(
            TokenType.Invalid,
            value,
            startPosition
        ) {
            Spec.Operators.TryGetValue(value, out Properties);
            Type = Properties.Type;
        }

        public OperatorToken(TokenType type) : base(type, Spec.OperatorTypes[type]) {
            Spec.Operators.TryGetValue(Value, out Properties);
            Type = Properties.Type;
        }
    }

    [JsonObject]
    public struct OperatorProperties {
        [JsonProperty]
        internal InputSide InputSide;

        [JsonProperty]
        internal readonly bool Overloadable;

        [JsonProperty]
        internal readonly int Precedence;

        [JsonProperty]
        internal readonly TokenType Type;

        internal OperatorProperties(
            TokenType type,
            int       precedence,
            InputSide inputSide    = InputSide.Both,
            bool      overloadable = false
        ) {
            Type         = type;
            Precedence   = precedence;
            InputSide    = inputSide;
            Overloadable = overloadable;
        }

        public override bool Equals(object obj) {
            if (obj is OperatorProperties properties) {
                return Equals(properties);
            }

            return false;
        }

        public bool Equals(OperatorProperties other) {
            return InputSide == other.InputSide
                   && Overloadable == other.Overloadable
                   && Precedence == other.Precedence
                   && Type == other.Type;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = Overloadable.GetHashCode();
                hashCode = (hashCode * 397) ^ Precedence;
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum InputSide {
        Unknown,
        Both,
        Right,
        Left
    }
}