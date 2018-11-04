using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Axion.Core.Tokens {
    [JsonObject]
    public class Token {
        [JsonProperty(Order = 1)] internal TokenType Type;

        [JsonProperty(Order = 2)] public string Value;

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

        public Token(TokenType type, (int line, int column) startPosition, string value = "") {
            Type        = type;
            Value       = value;
            StartLine   = startPosition.line;
            StartColumn = startPosition.column;
            string[] valueLines = value.Split(Spec.Newlines, StringSplitOptions.None);
            // compute end line & end column
            if (valueLines.Length == 1) {
                EndLine   = StartLine;
                EndColumn = StartColumn + value.Length;
            }
            else {
                EndLine   = StartLine + valueLines.Length - 1;
                EndColumn = valueLines[valueLines.Length - 1].Length;
            }
        }

        public virtual string ToAxionCode() {
            return Value;
        }

        public override string ToString() {
            return $"{Type}   ::   {Value}   ::   ({StartLine},{StartColumn}; {EndLine},{EndColumn})";
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