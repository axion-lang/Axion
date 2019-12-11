using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using Axion.Core.Specification;
using Newtonsoft.Json;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class Token : Span {
        [JsonProperty(Order = 1)]
        public TokenType Type { get; set; }

        [JsonProperty(Order = 2)]
        public string Value { get; protected set; }

        [JsonProperty(Order = 3)]
        public string Content { get; protected set; }

        [JsonProperty(Order = 4)]
        public string EndingWhite { get; set; }

        [JsonIgnore]
        public virtual TypeName ValueType { get; }

        [JsonIgnore]
        public readonly TextStream Stream;

        public Token(
            SourceUnit source,
            TokenType  type        = None,
            string     value       = "",
            string     content     = "",
            string     endingWhite = "",
            Location   start       = default,
            Location   end         = default
        ) : base(source, start, end) {
            Type        = type;
            Value       = value;
            Content     = string.IsNullOrEmpty(content) ? value : content;
            EndingWhite = endingWhite;
            Stream      = Source?.TextStream;
        }

        public bool Is(params TokenType[] types) {
            if (types.Length == 0) {
                return true;
            }

            for (var i = 0; i < types.Length; i++) {
                if (Type == types[i]) {
                    return true;
                }
            }

            return false;
        }

        public virtual Token Read() {
            Location startLoc = Stream.Location;
            Token    t        = ReadInternal();
            if (t != null) {
                t.MarkStart(startLoc);
                t.MarkEnd(Stream.Location);
            }

            return t;
        }

        private Token ReadInternal() {
            if (Stream.PeekIs(Spec.White)) {
                return ReadWhite();
            }

            if (Stream.PeekIs(Spec.IdStart)) {
                return ReadId();
            }

            if (Stream.PeekIs(Spec.PunctuationKeys)) {
                return ReadPunctuation();
            }

            if (Stream.PeekIs(Spec.OperatorsKeys)) {
                return new OperatorToken(Source).Read();
            }

            if (Stream.PeekIs(Spec.NumbersDec)) {
                return new NumberToken(Source).Read();
            }

            if (Stream.PeekIs(Spec.MultiLineCommentMark)) {
                return new CommentToken(Source).ReadMultiLine();
            }

            if (Stream.PeekIs(Spec.OneLineCommentMark)) {
                return new CommentToken(Source).ReadOneLine();
            }

            if (Stream.PeekIs(Spec.CharacterQuote)) {
                return new CharToken(Source).Read();
            }

            if (Stream.PeekIs(Spec.StringQuotes)) {
                return new StringToken(Source).Read();
            }

            if (Stream.PeekIs(Spec.Eols)) {
                return ReadNewline();
            }

            if (Stream.PeekIs(Spec.Eoc)) {
                return ReadEoc();
            }

            // invalid character token
            Type = Invalid;
            AppendNext();
            LangException.Report(BlameType.InvalidCharacter, this);
            return this;
        }

        public bool AppendNext(bool content = false, params string[] expected) {
            string eaten = Stream.Eat(expected) ?? "";
            Value += eaten;
            if (content) {
                Content += eaten;
            }

            return eaten != "";
        }

        public Token ReadNewline() {
            Type = Newline;
            while (AppendNext(expected: Spec.Eols)) { }

            if (Stream.PeekIs(Spec.White)) {
                return this;
            }

            // root-level newline - reset indentation
            Source.TokenStream.Tokens.Add(this);
            Source.LastIndentLen = 0;
            while (Source.IndentLevel > 0) {
                Source.TokenStream.Tokens.Add(
                    new Token(
                        Source,
                        Outdent,
                        start: Stream.Location
                    )
                );
                Source.IndentLevel--;
            }

            return null;
        }

        public Token ReadWhite() {
            while (AppendNext(expected: Spec.White)) { }

            if (Source.TokenStream.Tokens.Count == 0) {
                Source.LastIndentLen = Value.Length;
                Type                 = Whitespace;
                return this;
            }

            string ln = Stream.RestOfLine;
            if (Source.TokenStream.Tokens.Last().Is(Newline) && !(
                    string.IsNullOrWhiteSpace(ln)
                 || ln.StartsWith(Spec.OneLineCommentMark)
                 || Source.MismatchingPairs.Count > 0
                 || Source.TokenStream.Tokens[^2] is OperatorToken op &&
                    op.Side == InputSide.Both
                 || Spec.NotIndentRegex.IsMatch(ln))
            ) {
                if (Source.IndentChar == '\0') {
                    Source.IndentChar = Value[0];
                }

                var      consistent         = true;
                Location inconsistencyStart = default;
                var      newIndentLen       = 0;
                for (var i = 0; i < Value.Length; i++) {
                    char ch = Value[i];
                    if (consistent && ch != Source.IndentChar) {
                        inconsistencyStart = new Location(Start.Line, i);
                        consistent         = false;
                    }

                    if (ch == ' ') {
                        newIndentLen++;
                    }
                    else if (ch == '\t') {
                        newIndentLen += Source.IndentSize == 0 ? 8 : Source.IndentSize;
                    }
                }

                if (consistent) {
                    if (Source.IndentSize == 0) {
                        Source.IndentSize = newIndentLen;
                    }
                }
                else {
                    LangException.Report(
                        BlameType.InconsistentIndentation,
                        new Span(Source, inconsistencyStart, End)
                    );
                }

                Token result = null;
                if (newIndentLen > Source.LastIndentLen) {
                    Type = Indent;
                    Source.IndentLevel++;
                    result = this;
                }
                else if (newIndentLen < Source.LastIndentLen) {
                    Type = Outdent;
                    while (newIndentLen < Source.LastIndentLen) {
                        Source.TokenStream.Tokens.Add(this);
                        Source.IndentLevel--;
                        Source.LastIndentLen -= Source.IndentSize;
                    }
                }

                Source.LastIndentLen = newIndentLen;
                return result;
            }

            Source.TokenStream.Tokens[^1].EndingWhite += Value;
            return null;
        }

        public Token ReadEoc() {
            Type = TokenType.End;
            AppendNext(expected: Spec.Eoc);
            return this;
        }

        public Token ReadId() {
            do {
                AppendNext(true);
            } while (Stream.PeekIs(Spec.IdPart)
                  && (!Stream.PeekIs(Spec.IdNotEnd)
                   || Spec.IdAfterNotEnd.Contains(Stream.Peek(2)[1].ToString())));

            if (Stream.PeekIs(Spec.StringQuotes)) {
                // TODO: check for prefixes
                return new StringToken(Source, prefixes: Value).Read();
            }

            if (Spec.Keywords.ContainsKey(Value)) {
                Type = Spec.Keywords[Value];
            }
            else if (Spec.OperatorsKeys.Contains(Value)) {
                return new OperatorToken(Source, Value, start: Start, end: End);
            }
            else {
                Type  = Identifier;
                Value = Content = Value.Replace("-", "_");
            }

            return this;
        }

        public Token ReadPunctuation() {
            AppendNext(expected: Spec.PunctuationKeys);
            Type = Spec.Punctuation[Value];
            if (Type.IsOpenBracket()) {
                Source.MismatchingPairs.Push(this);
            }
            else if (Type.IsCloseBracket()) {
                if (Source.MismatchingPairs.Count                             > 0
                 && Source.MismatchingPairs.First().Type.GetMatchingBracket() == Type) {
                    Source.MismatchingPairs.Pop();
                }
                else {
                    Source.MismatchingPairs.Push(this);
                }
            }

            return this;
        }

        public void ReadEscapeSeq() {
            string   raw         = Stream.Eat(Spec.EscapeMark);
            var      escaped     = "";
            Location escapeStart = Stream.Location;
            // \t, \n, etc
            if (Stream.Eat(Spec.EscapeSequences.Keys.ToArray()) != null) {
                raw     += Stream.C;
                escaped += Spec.EscapeSequences[Stream.C];
            }
            // \u h{4} of \U h{8}
            else if (Stream.Eat("u", "U") != null) {
                string u = Stream.C;
                raw += u;
                int uEscapeLen = u == "u" ? 4 : 8;
                var escapeLen  = 0;
                while (escapeLen < uEscapeLen) {
                    if (Stream.Eat(Spec.NumbersHex) != null) {
                        raw += Stream.C;
                        escapeLen++;
                    }
                    else {
                        RaiseError(BlameType.TruncatedEscapeSequence);
                        return;
                    }
                }

                int? num = Utilities.ParseInt(raw.Substring(2), 16);
                if (num != null) {
                    if (0 <= num && num <= 0x10ffff) {
                        escaped += num;
                    }
                    else {
                        RaiseError(BlameType.IllegalUnicodeCharacter);
                        return;
                    }
                }
                else {
                    RaiseError(BlameType.InvalidEscapeSequence);
                    return;
                }
            }
            else if (Stream.Eat("x") != null) {
                raw += Stream.C;
                while (Stream.Eat(Spec.NumbersHex) != null && raw.Length < 4) {
                    raw += Stream.C;
                }

                if (raw.Length != 4) {
                    RaiseError(BlameType.InvalidXEscapeFormat);
                    return;
                }

                int? num = Utilities.ParseInt(raw.Substring(2), 16);
                if (num != null) {
                    escaped += num;
                }
                else {
                    RaiseError(BlameType.InvalidEscapeSequence);
                    return;
                }
            }
            else if (Stream.AtEndOfLine) {
                RaiseError(BlameType.TruncatedEscapeSequence);
                return;
            }
            else {
                RaiseError(BlameType.InvalidEscapeSequence);
                return;
            }

            void RaiseError(BlameType blameType) {
                LangException.Report(
                    blameType,
                    new Span(Source, escapeStart, Stream.Location)
                );
                Value   += raw;
                Content += raw;
            }

            Value   += raw;
            Content += escaped;
        }

        public override string ToString() {
            return Type
                 + " :: "
                 + Value.Replace("\r", "\\r")
                        .Replace("\n", "\\n")
                        .Replace("\t", "\\t")
                 + " :: "
                 + base.ToString();
        }

        protected bool Equals(Token other) {
            return Type == other.Type
                && string.Equals(Value,       other.Value)
                && string.Equals(EndingWhite, other.EndingWhite);
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
                hashCode = (hashCode * 397) ^ EndingWhite.GetHashCode();
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

        public override void ToAxion(CodeWriter c) {
            c.Write(Value);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(Value);
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Value);
        }
    }
}