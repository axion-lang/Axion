using System;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    [JsonObject]
    public class Token : SpannedRegion {
        [JsonProperty(Order = 1)]
        public TokenType Type { get; protected internal set; }

        [JsonProperty(Order = 2)]
        public string Value { get; protected set; }

        /// <summary>
        ///     Whitespaces after that token.
        ///     Used in code rendering.
        /// </summary>
        [JsonProperty(Order = 3)]
        public string Whitespaces { get; protected internal set; } = "";

        public Token(string value) {
            Value = value;
        }

        public Token(TokenType type, string value = "") {
            Type  = type;
            Value = value;
        }

        public Token(
            TokenType type,
            string    value         = "",
            Position  startPosition = default
        ) {
            Type  = type;
            Value = value;

            // compute end line & end column
            int      endLine;
            int      endCol;
            string[] valueLines = value.Split(Spec.EndOfLines, StringSplitOptions.None);
            if (valueLines.Length == 1) {
                endLine = startPosition.Line;
                endCol  = startPosition.Column + value.Length;
            }
            else {
                endLine = startPosition.Line + valueLines.Length - 1;
                endCol  = valueLines[valueLines.Length - 1].Length;
            }

            Span = new Span(startPosition, (endLine, endCol));
        }

        public bool Is(params TokenType[] types) {
            for (var i = 0; i < types.Length; i++) {
                if (Type == types[i]) {
                    return true;
                }
            }

            return false;
        }

        public Token AppendValue(string value) {
            Value += value;
            string[] valueLines = value.Split(Spec.EndOfLines, StringSplitOptions.None);

            // compute end line & end column
            int endLine = Span.EndPosition.Line;
            int endCol  = Span.EndPosition.Column;
            if (valueLines.Length == 1) {
                endCol += value.Length;
            }
            else {
                endLine += valueLines.Length - 1;
                endCol  =  valueLines[valueLines.Length - 1].Length;
            }

            Span = new Span(Span.StartPosition, (endLine, endCol));
            return this;
        }

        public Token AppendWhitespace(string space) {
#if DEBUG
            var whites = new[] {
                ' ', '\t'
            };
            Debug.Assert(space.All(c => whites.Contains(c)));
#endif
            Whitespaces += space;
            return this;
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in Axion language format.
        /// </summary>
        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Value + Whitespaces;
        }

        public override string ToString() {
            return
                $"{Type} :: {Value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")} :: ({Span})";
        }
    }
}