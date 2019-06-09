using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'operator' literal.
    /// </summary>
    public class OperatorToken : Token {
        internal OperatorProperties Properties;

        public OperatorToken(string value, Position startPosition = default)
            : base(
                TokenType.Invalid,
                value,
                startPosition
            ) {
            Spec.Operators.TryGetValue(Value, out Properties);
            // overrides 'Invalid'
            Type = Properties.Type;
        }

        public OperatorToken(TokenType type) : this(type.GetValue()) { }
    }

    public struct OperatorProperties {
        [JsonProperty]
        internal InputSide InputSide;

        public readonly bool      AllowOverload;
        public readonly int       Precedence;
        public readonly TokenType Type;

        internal OperatorProperties(
            TokenType type,
            int       precedence,
            InputSide inputSide     = InputSide.Both,
            bool      allowOverload = false
        ) {
            Type          = type;
            Precedence    = precedence;
            InputSide     = inputSide;
            AllowOverload = allowOverload;
        }

        public override bool Equals(object obj) {
            if (obj is OperatorProperties properties) {
                return Equals(properties);
            }

            return false;
        }

        public bool Equals(OperatorProperties other) {
            return InputSide == other.InputSide
                   && AllowOverload == other.AllowOverload
                   && Precedence == other.Precedence
                   && Type == other.Type;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = AllowOverload.GetHashCode();
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