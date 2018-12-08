using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    [JsonObject]
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class Token {
        [JsonProperty(Order = 1)]
        public TokenType Type { get; protected internal set; }

        [JsonProperty(Order = 2)]
        public string Value { get; protected internal set; }

        /// <summary>
        ///     Whitespaces after that token.
        ///     Used in code rendering.
        /// </summary>
        [JsonProperty(Order = 3)]
        public string Whitespaces { get; protected internal set; }

        /// <summary>
        ///     Line position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1001)]
        public int StartLine { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> start in the input stream.
        /// </summary>
        [JsonProperty(Order = 1002)]
        public int StartColumn { get; protected internal set; }

        /// <summary>
        ///     Line position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1003)]
        public int EndLine { get; protected internal set; }

        /// <summary>
        ///     Column position of <see cref="Token" /> end in the input stream.
        /// </summary>
        [JsonProperty(Order = 1004)]
        public int EndColumn { get; protected internal set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay =>
            $"{Type} :: {Value.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t")} :: ({StartLine},{StartColumn}; {EndLine},{EndColumn})";

        public Token(TokenType type, (int line, int column) startPosition, string value = "", string whitespaces = "") {
            Type        = type;
            Value       = value;
            Whitespaces = whitespaces;
            StartLine   = startPosition.line;
            StartColumn = startPosition.column;
            string[] valueLines = value.Split(Spec.EndOfLines, StringSplitOptions.None);
            // compute end line & end column
            if (valueLines.Length == 1) {
                EndLine   = StartLine;
                EndColumn = StartColumn + value.Length;
            }
            else {
                EndLine   = StartLine + valueLines.Length - 1;
                EndColumn = valueLines[valueLines.Length - 1].Length;
            }
            EndColumn += Whitespaces.Length;
        }

        public void AppendWhitespace(string space) {
            Whitespaces += space;
            EndColumn   += space.Length;
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in Axion language format.
        /// </summary>
        /// <returns></returns>
        public virtual string ToAxionCode() {
            return Value + Whitespaces;
        }

        public override string ToString() {
            return $"{Type} :: {Value} :: ({StartLine},{StartColumn}; {EndLine},{EndColumn})";
        }

        public override bool Equals(object obj) {
            return obj is Token token
                && StartLine == token.StartLine
                && StartColumn == token.StartColumn
                && EndLine == token.EndLine
                && EndColumn == token.EndColumn
                && Type == token.Type
                && Value == token.Value;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            int hashCode = -276116195;
            hashCode = hashCode * -1521134295 + StartLine.GetHashCode();
            hashCode = hashCode * -1521134295 + StartColumn.GetHashCode();
            hashCode = hashCode * -1521134295 + EndLine.GetHashCode();
            hashCode = hashCode * -1521134295 + EndColumn.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}