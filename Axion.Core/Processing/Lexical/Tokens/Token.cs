using System;
using System.Diagnostics;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    [JsonObject]
    [DebuggerDisplay("{debuggerDisplay,nq}")]
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
        public string Whitespaces { get; protected internal set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay =>
            $"{Type} :: {Value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")} :: ({Span})";

        public Token(TokenType type, Position startPosition, string value = "", string whitespaces = "") {
            Type        = type;
            Value       = value;
            Whitespaces = whitespaces;

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

        public void AppendValue(string value) {
            Value += value;
            string[] valueLines = value.Split(Spec.EndOfLines, StringSplitOptions.None);

            // compute end line & end column
            int endLine = Span.End.Line;
            int endCol  = Span.End.Column;
            if (valueLines.Length == 1) {
                endCol += value.Length;
            }
            else {
                endLine += valueLines.Length - 1;
                endCol  =  valueLines[valueLines.Length - 1].Length;
            }
            Span = new Span(Span.Start, (endLine, endCol));
        }

        public void AppendWhitespace(string space) {
            Whitespaces += space;
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in Axion language format.
        /// </summary>
        public virtual string ToAxionCode() {
            return Value + Whitespaces;
        }

        public override string ToString() {
            return $"{Type} :: {Value} :: ({Span})";
        }
    }
}