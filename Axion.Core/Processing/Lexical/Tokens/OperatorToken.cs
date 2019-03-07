using System.Linq;
using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'operator' literal.
    /// </summary>
    public class OperatorToken : Token {
        internal OperatorProperties Properties;

        public OperatorToken(Position startPosition, OperatorProperties properties) : base(
            properties.Type,
            startPosition
        ) {
            Properties = properties;
        }

        public OperatorToken(Position startPosition, string value, string whitespaces = "") : base(
            TokenType.Invalid,
            startPosition,
            value,
            whitespaces
        ) {
            Spec.Operators.TryGetValue(value, out Properties);
            Type = Properties.Type;
        }

        public override string ToAxionCode() {
            return Spec.Operators.First(kvp => kvp.Value.Equals(Properties)).Key + Whitespaces;
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
            bool      overloadable,
            int       precedence
        ) {
            InputSide    = InputSide.Unknown;
            Type         = type;
            Overloadable = overloadable;
            Precedence   = precedence;
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
                var hashCode = (int) InputSide;
                hashCode = (hashCode * 397) ^ Overloadable.GetHashCode();
                hashCode = (hashCode * 397) ^ Precedence;
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum InputSide {
        Unknown,
        Right,
        Left
    }
}