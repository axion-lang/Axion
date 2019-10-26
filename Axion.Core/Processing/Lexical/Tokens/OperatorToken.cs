using System.Linq;
using Axion.Core.Source;
using Axion.Core.Specification;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class OperatorToken : Token {
        public int Precedence { get; private set; }

        public InputSide Side { get; set; }

        public OperatorToken(
            SourceUnit source,
            string     value       = "",
            string     endingWhite = "",
            TokenType  tokenType   = TokenType.Unknown,
            int        precedence  = -1,
            InputSide  side        = InputSide.Unknown,
            Location   start       = default,
            Location   end         = default
        ) : base(source, tokenType, value, endingWhite: endingWhite, start: start, end: end) {
            if (tokenType != TokenType.Unknown) {
                Value = Content = Spec.Operators.First(kvp => kvp.Value.Item1 == tokenType).Key;
            }
            else if (!string.IsNullOrWhiteSpace(Value)) {
                Type = Spec.Operators[Value].Item1;
            }
            else {
                Precedence = -1;
                Side       = InputSide.Unknown;
                return;
            }

            Precedence = precedence == -1
                ? Spec.Operators[Value].Item2
                : precedence;
            Side = side == InputSide.Unknown
                ? Spec.Operators[Value].Item3
                : side;
        }

        public override Token Read() {
            AppendNext(true, Spec.OperatorsKeys);
            if (Value != null) {
                (Type, Precedence, Side) = Spec.Operators[Value];
            }

            return this;
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