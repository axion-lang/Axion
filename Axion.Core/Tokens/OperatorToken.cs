using System;
using System.Linq;
using Axion.Core.Processing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;operator&gt; <see cref="Token" />.
    /// </summary>
    public class OperatorToken : Token {
        internal readonly OperatorProperties Properties;

        public OperatorToken((int, int) startPosition, OperatorProperties properties)
            : base(properties.Type, startPosition) {
            Properties = properties;
        }

        public OperatorToken((int, int) startPosition, string value)
            : base(TokenType.Unknown, startPosition, value) {
            Spec.Operators.TryGetValue(value, out Properties);
            Type = Properties.Type;
        }

        public override string ToAxionCode() {
            return Spec.Operators.First(kvp => kvp.Value.Equals(Properties)).Key;
        }
    }

    [JsonObject]
    public struct OperatorProperties {
        [JsonProperty] internal readonly InputSide InputSide;

        [JsonProperty] internal readonly Associativity Associativity;

        [JsonProperty] internal readonly bool Overloadable;

        [JsonProperty] internal readonly int Precedence;

        [JsonProperty] internal readonly TokenType Type;

        internal OperatorProperties(
            TokenType     type,
            InputSide     inputSide,
            Associativity associativity,
            bool          overloadable,
            int           precedence
        ) {
            Type          = type;
            InputSide     = inputSide;
            Associativity = associativity;
            Overloadable  = overloadable;
            Precedence    = precedence;
        }

        internal ErrorType PossibleErrorType {
            get {
                switch (Type) {
                    case TokenType.OpLeftParenthesis:
                    case TokenType.OpRightParenthesis: return ErrorType.MismatchedParenthesis;
                    case TokenType.OpLeftBracket:
                    case TokenType.OpRightBracket: return ErrorType.MismatchedBracket;
                    case TokenType.OpLeftBrace:
                    case TokenType.OpRightBrace: return ErrorType.MismatchedBrace;
                    default: return ErrorType.InvalidOperator;
                }
            }
        }

        internal bool IsOpenBrace => Type == TokenType.OpLeftParenthesis ||
                                     Type == TokenType.OpLeftBracket ||
                                     Type == TokenType.OpLeftBrace;

        internal bool IsCloseBrace => Type == TokenType.OpRightParenthesis ||
                                      Type == TokenType.OpRightBracket ||
                                      Type == TokenType.OpRightBrace;

        internal TokenType MatchingBrace {
            get {
                switch (Type) {
                    // open : close
                    case TokenType.OpLeftParenthesis: return TokenType.OpRightParenthesis;
                    case TokenType.OpLeftBracket:     return TokenType.OpRightBracket;
                    case TokenType.OpLeftBrace:       return TokenType.OpRightBrace;
                    // close : open
                    case TokenType.OpRightParenthesis: return TokenType.OpLeftParenthesis;
                    case TokenType.OpRightBracket:     return TokenType.OpLeftBracket;
                    case TokenType.OpRightBrace:       return TokenType.OpLeftBrace;
                    // should never be
                    default: throw new Exception();
                }
            }
        }

        public override bool Equals(object obj) {
            if (obj is OperatorProperties properties) {
                return Equals(properties);
            }
            return false;
        }

        public bool Equals(OperatorProperties other) {
            return InputSide == other.InputSide &&
                   Associativity == other.Associativity &&
                   Overloadable == other.Overloadable &&
                   Precedence == other.Precedence &&
                   Type == other.Type;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) InputSide;
                hashCode = (hashCode * 397) ^ (int) Associativity;
                hashCode = (hashCode * 397) ^ Overloadable.GetHashCode();
                hashCode = (hashCode * 397) ^ Precedence;
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum InputSide {
        None,
        Left,
        Right,
        Both,
        SomeOne
    }

    [JsonConverter(typeof(StringEnumConverter))]
    internal enum Associativity {
        RightToLeft,
        LeftToRight,
        None
    }
}