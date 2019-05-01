using System;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Lexical.Tokens {
    [JsonObject]
    public class Token : SpannedRegion {
        [JsonProperty(Order = 1)]
        public TokenType Type { get; protected internal set; }

        [JsonProperty(Order = 2)]
        public string Value { get; private set; }

        [JsonProperty(Order = 3)]
        public string EndWhitespaces { get; private set; } = "";

        /// <summary>
        ///     Constructor for token that has no length
        ///     (such as 'end of code' or 'outdent')
        /// </summary>
        public Token(TokenType type, Position startPos) {
            Type  = type;
            Value = "";
            Span  = new Span(startPos, startPos);
        }

        /// <summary>
        ///     Constructor for token without
        ///     position in source.
        /// </summary>
        public Token(TokenType type, string value = "") {
            Type  = type;
            Value = value;
        }

        /// <summary>
        ///     Constructor for Lexer and unit testing.
        /// </summary>
        public Token(TokenType type, string value = "", Position startPos = default) {
            Type  = type;
            Value = value;

            // compute position
            int      endLine;
            int      endCol;
            string[] valueLines = value.Split('\n');
            if (valueLines.Length == 1) {
                endLine = startPos.Line;
                endCol  = startPos.Column + value.Length;
            }
            else {
                endLine = startPos.Line + valueLines.Length - 1;
                endCol  = valueLines[valueLines.Length - 1].Length;
            }

            Span = new Span(startPos, (endLine, endCol));
        }

        public bool ShouldSerializeEndWhitespaces() {
            return !Compiler.Options.HasFlag(SourceProcessingOptions.ShowAstJson);
        }

        /// <summary>
        ///     Checks if token is of any
        ///     of specified <paramref name="types"/>.
        /// </summary>
        public bool Is(params TokenType[] types) {
            for (var i = 0; i < types.Length; i++) {
                if (Type == types[i]) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Appends specified <paramref name="value"/>
        ///     to token's value.
        /// </summary>
        internal Token AppendValue(string value) {
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

        /// <summary>
        ///     Appends specified <paramref name="space"/>
        ///     to token's end whitespaces.
        /// </summary>
        public Token AppendWhitespace(string space) {
#if DEBUG
            var whites = new[] {
                ' ', '\t'
            };
            Debug.Assert(space.All(c => whites.Contains(c)));
#endif
            EndWhitespaces += space;
            return this;
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in Axion language format,
        ///     with original formatting of whitespaces.
        /// </summary>
        internal string ToOriginalAxionCode() {
            var b = new CodeBuilder(OutLang.Axion);
            ToAxionCode(b);
            b.Write(EndWhitespaces);
            return b.ToString();
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in Axion language format.
        /// </summary>
        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Value);
        }

        /// <summary>
        ///     Returns string representation of
        ///     this token in C# language format.
        /// </summary>
        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Value);
        }

        public override string ToString() {
            return Type
                   + " :: "
                   + Value.Replace("\r", "\\r")
                          .Replace("\n", "\\n")
                          .Replace("\t", "\\t")
                   + " :: "
                   + Span;
        }

        protected bool Equals(Token other) {
            return Type == other.Type
                   && string.Equals(Value, other.Value)
                   && string.Equals(EndWhitespaces, other.EndWhitespaces);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Token) obj);
        }

        public override int GetHashCode() {
            unchecked {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ EndWhitespaces.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }

        public static bool operator ==(Token left, Token right) {
            return Equals(left, right);
        }

        public static bool operator !=(Token left, Token right) {
            return !Equals(left, right);
        }
    }
}