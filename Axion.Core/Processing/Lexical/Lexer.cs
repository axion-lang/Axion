using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Specification;
using static Axion.Specification.TokenType;
using static Axion.Specification.Spec;

namespace Axion.Core.Processing.Lexical {
    public class Lexer {
        readonly Unit unit;
        readonly TextStream stream;

        // Variables for current token creation
        TokenType type;
        readonly StringBuilder value = new();
        readonly StringBuilder content = new();
        Location startLoc;

        // Variables for indentation analysis
        char indentChar;
        int indentSize;
        int lastIndentLen;
        int indentLevel;

        public Stack<Token> MismatchingPairs { get; } = new();

        public Stack<TokenType> ProcessTerminators { get; } = new();

        public Lexer(Unit unit) {
            this.unit = unit;
            stream    = this.unit.TextStream;
            ProcessTerminators.Push(End);
        }

        bool TryAddChar(char expected, bool isContent = true) {
            return TryAddChar(new[] { expected }, isContent);
        }

        bool TryAddChar(char[] expect, bool isContent = true) {
            var eaten = stream.Eat(expect) ?? "";
            value.Append(eaten);
            if (isContent) {
                content.Append(eaten);
            }

            return eaten != "";
        }

        bool AddNext(bool isContent = true, params string[] expect) {
            var eaten = stream.Eat(expect) ?? "";
            value.Append(eaten);
            if (isContent) {
                content.Append(eaten);
            }

            return eaten != "";
        }

        T BindSpan<T>(T token) where T : Token {
            token.MarkStart(startLoc);
            token.MarkEnd(stream.Location);
            return token;
        }

        Token NewTokenFromContext() {
            var token = new Token(
                unit,
                type,
                value.ToString(),
                content.ToString()
            );
            token.MarkStart(startLoc);
            token.MarkEnd(stream.Location);
            return token;
        }

        public Token? Read() {
            startLoc = stream.Location;
            type     = Invalid;
            value.Clear();
            content.Clear();
            return ReadInternal();
        }

        Token? ReadInternal() {
            if (stream.PeekIs(White)) {
                return ReadWhite();
            }

            if (stream.Peek().IsValidIdStart()) {
                return ReadId();
            }

            if (stream.PeekIs(PunctuationKeys)) {
                return ReadPunctuation();
            }

            if (stream.PeekIs(OperatorsKeys)) {
                return ReadOperator();
            }

            if (stream.PeekIs(NumbersDec)) {
                return ReadNumber();
            }

            if (stream.PeekIs(MultiLineCommentMark)) {
                return ReadMultiLineComment();
            }

            if (stream.PeekIs(OneLineCommentMark)) {
                return ReadOneLineComment();
            }

            if (stream.PeekIs(CharacterQuote)) {
                return ReadChar();
            }

            if (stream.PeekIs(StringQuotes)) {
                return ReadString();
            }

            if (stream.PeekIs(Eols)) {
                return ReadNewline();
            }

            if (stream.PeekIs(EndOfCode)) {
                return ReadEoc();
            }

            // invalid character token
            AddNext();
            var t = NewTokenFromContext();
            LanguageReport.To(BlameType.InvalidCharacter, t);
            return t;
        }

        Token? ReadWhite() {
            bool addNext;
            do {
                addNext = TryAddChar(White);
            } while (addNext);

            if (unit.TokenStream.Count == 0) {
                lastIndentLen = value.Length;
                type          = Whitespace;
                return NewTokenFromContext();
            }

            var ln = stream.RestOfLine;
            // check that whitespace is not meaningful here
            if (!unit.TokenStream[^1].Is(Newline)
             || ln.Trim().Length == 0
             || ln.StartsWith(OneLineCommentMark)
             || MismatchingPairs.Count > 0
             || unit.TokenStream[^2] is OperatorToken { Side: InputSide.Both }
             || NonIndentRegex.IsMatch(ln)) {
                unit.TokenStream[^1].EndingWhite += value;
                return null;
            }

            // indentation processing
            if (indentChar == default) {
                indentChar = value[0];
            }

            var consistent = true;
            Location inconsistencyStart = default;
            var newIndentLen = 0;
            for (var i = 0; i < value.Length; i++) {
                var ch = value[i];
                if (consistent && ch != indentChar) {
                    inconsistencyStart = new Location(startLoc.Line, i);
                    consistent         = false;
                }

                if (ch == ' ') {
                    newIndentLen++;
                }
                else if (ch == '\t') {
                    newIndentLen += indentSize == 0 ? 8 : indentSize;
                }
            }

            if (!consistent) {
                LanguageReport.To(
                    BlameType.InconsistentIndentation,
                    new CodeSpan(
                        unit,
                        inconsistencyStart,
                        stream.Location - (0, 1)
                    )
                );
            }
            else {
                if (indentSize == 0) {
                    indentSize = newIndentLen;
                }
            }

            if (newIndentLen > lastIndentLen) {
                type = Indent;
                indentLevel++;
                lastIndentLen = newIndentLen;
                return NewTokenFromContext();
            }

            if (newIndentLen < lastIndentLen) {
                type = Outdent;
                while (newIndentLen < lastIndentLen) {
                    indentLevel--;
                    lastIndentLen -= indentSize;
                    unit.TokenStream.Add(NewTokenFromContext());
                    value.Clear();
                    content.Clear();
                }
            }
            else {
                unit.TokenStream[^1].EndingWhite += value;
            }

            lastIndentLen = newIndentLen;
            return null;
        }

        Token ReadId() {
            do {
                AddNext();
            } while (stream.Peek().IsValidIdPart()
                  && (!stream.Peek().IsValidIdNonEnd()
                   || stream.Peek(2)[1].IsValidIdEnd()));

            if (stream.PeekIs(StringQuotes)) {
                return ReadString();
            }

            if (unit.Module?.CustomKeywords.Contains(value.ToString())
             ?? false) {
                type = CustomKeyword;
                return NewTokenFromContext();
            }

            if (Keywords.TryGetValue(value.ToString(), out type)) {
                return NewTokenFromContext();
            }

            if (OperatorsKeys.Contains(value.ToString())) {
                return BindSpan(new OperatorToken(unit, value.ToString()));
            }

            type = Identifier;
            content.Replace("-", "_");

            return NewTokenFromContext();
        }

        Token? ReadNewline() {
            type = Newline;

            bool addNext;
            do {
                addNext = TryAddChar(Eols);
            } while (addNext);

            unit.TokenStream.Add(NewTokenFromContext());
            value.Clear();
            content.Clear();

            if (!unit.TextStream.PeekIs(White)) {
                // root-level newline - reset indentation
                lastIndentLen = 0;
                while (indentLevel > 0) {
                    type     = Outdent;
                    startLoc = stream.Location;
                    unit.TokenStream.Add(NewTokenFromContext());
                    indentLevel--;
                }
            }

            return null;
        }

        Token ReadEoc() {
            type = End;
            TryAddChar(EndOfCode);
            return NewTokenFromContext();
        }

        NumberToken ReadNumber() {
            bool addNext;
            do {
                addNext = TryAddChar('0');
            } while (addNext);

            while (TryAddChar(NumbersDec)) {
                content.Append(stream.Char);
            }

            return BindSpan(
                new NumberToken(unit, value.ToString(), content.ToString())
            );
        }

        OperatorToken ReadOperator() {
            AddNext(expect: OperatorsKeys);
            return BindSpan(new OperatorToken(unit, value.ToString()));
        }

        Token ReadPunctuation() {
            AddNext(expect: PunctuationKeys);
            type = Punctuation[value.ToString()];
            var t = NewTokenFromContext();
            if (type.IsOpenBracket()) {
                MismatchingPairs.Push(t);
            }
            else if (type.IsCloseBracket()) {
                if (MismatchingPairs.Count > 0
                 && MismatchingPairs.First().Type.GetMatchingBracket()
                 == type) {
                    MismatchingPairs.Pop();
                }
                else {
                    MismatchingPairs.Push(t);
                }
            }

            return t;
        }

        CharToken ReadChar() {
            AddNext(false, CharacterQuote);
            while (!AddNext(false, CharacterQuote)) {
                if (stream.AtEndOfLine) {
                    var unclosed = BindSpan(
                        new CharToken(
                            unit,
                            value.ToString(),
                            content.ToString(),
                            true
                        )
                    );
                    LanguageReport.To(
                        BlameType.UnclosedCharacterLiteral,
                        unclosed
                    );
                    return unclosed;
                }

                if (stream.PeekIs('\\')) {
                    ReadEscapeSeq();
                }
                else {
                    AddNext();
                }
            }

            var t = BindSpan(
                new CharToken(unit, value.ToString(), content.ToString())
            );

            if (content.Length == 0) {
                LanguageReport.To(BlameType.EmptyCharacterLiteral, t);
            }
            else if (content.Replace("\\", "").Length != 1) {
                LanguageReport.To(BlameType.CharacterLiteralTooLong, t);
            }

            return t;
        }

        CommentToken ReadOneLineComment() {
            AddNext(false, OneLineCommentMark);
            while (!stream.AtEndOfLine) {
                AddNext();
            }

            return BindSpan(
                new CommentToken(unit, value.ToString(), content.ToString())
            );
        }

        CommentToken ReadMultiLineComment() {
            AddNext(false, MultiLineCommentMark);
            while (!stream.PeekIs(MultiLineCommentMark)) {
                if (stream.PeekIs(EndOfCode)) {
                    var t = BindSpan(
                        new CommentToken(
                            unit,
                            value.ToString(),
                            content.ToString(),
                            true,
                            true
                        )
                    );
                    LanguageReport.To(BlameType.UnclosedMultilineComment, t);
                    return t;
                }

                AddNext();
            }

            AddNext(false, MultiLineCommentMark);
            return BindSpan(
                new CommentToken(
                    unit,
                    value.ToString(),
                    content.ToString(),
                    true
                )
            );
        }

        Token ReadString() {
            var prefixes = "";
            if (!string.IsNullOrWhiteSpace(value.ToString())) {
                // if string has prefixes, then
                // ReadString was called from ReadId,
                // and content == string prefixes.
                prefixes = content.ToString();
                value.Clear().Append(prefixes);
                content.Clear();
                for (var i = 0; i < prefixes.Length; i++) {
                    var p = prefixes[i];
                    if (StringPrefixes.Contains(char.ToLowerInvariant(p))) {
                        continue;
                    }

                    var ps = p.ToString();
                    var token = BindSpan(
                        new Token(
                            unit,
                            Invalid,
                            ps,
                            ps,
                            "",
                            stream.Location + (0, i)
                        )
                    );
                    LanguageReport.To(BlameType.InvalidStringPrefix, token);
                }
            }

            TryAddChar(StringQuotes, false);
            var quote = value[^1].ToString();
            var closingQuote = quote;
            if (AddNext(false, quote.Multiply(MultilineStringQuotesCount - 1))) {
                quote = quote.Multiply(MultilineStringQuotesCount);
            }
            else if (AddNext(false, quote)) {
                var se = BindSpan(
                    new StringToken(
                        unit,
                        value.ToString(),
                        content.ToString(),
                        false,
                        prefixes,
                        quote
                    )
                );
                if (prefixes.Length > 0) {
                    LanguageReport.To(
                        BlameType.RedundantPrefixesForEmptyString,
                        se
                    );
                }

                return se;
            }

            var interpolations = new List<StringInterpolation>();
            var unclosed = false;
            while (true) {
                if (stream.PeekIs('\\') && !prefixes.Contains("r")) {
                    ReadEscapeSeq();
                }
                else if (stream.PeekIs(Eols) && quote.Length == 1
                      || stream.PeekIs(EndOfCode)) {
                    unclosed = true;
                    break;
                }
                else if (quote.Length == 1 && stream.PeekIs(closingQuote)) {
                    AddNext(false, closingQuote);
                    break;
                }
                else if (AddNext(
                    false,
                    closingQuote.Multiply(MultilineStringQuotesCount)
                )) {
                    break;
                }
                else if (prefixes.Contains("f") && stream.PeekIs("{")) {
                    interpolations.Add(ReadStringInterpolation());
                }
                else {
                    AddNext();
                }
            }

            var s = BindSpan(
                new StringToken(
                    unit,
                    value.ToString(),
                    content.ToString(),
                    unclosed,
                    prefixes,
                    quote,
                    interpolations
                )
            );
            if (prefixes.Contains("f") && interpolations.Count == 0) {
                LanguageReport.To(BlameType.RedundantStringFormat, s);
            }

            if (unclosed) {
                LanguageReport.To(BlameType.UnclosedString, s);
            }

            return s;
        }

        StringInterpolation ReadStringInterpolation() {
            var interpolation = new StringInterpolation(unit);
            var iSrc = interpolation.Unit;
            var lexer = new Lexer(interpolation.Unit);

            lexer.ProcessTerminators.Push(CloseBrace);
            while (true) {
                var token = lexer.Read();
                if (token == null) {
                    continue;
                }

                iSrc.TokenStream.Add(token);
                if (lexer.ProcessTerminators.Contains(token.Type)) {
                    break;
                }
            }

            foreach (var mismatch in lexer.MismatchingPairs) {
                LanguageReport.MismatchedBracket(mismatch);
            }

            // remove '{' '}'
            iSrc.TokenStream.RemoveAt(0);
            iSrc.TokenStream.RemoveAt(iSrc.TokenStream.Count - 1);
            return interpolation;
        }

        void ReadEscapeSeq() {
            var escapeStart = stream.Location;
            var raw = stream.Eat('\\')!;
            var escaped = "";
            // \t, \n, etc
            if (stream.Eat(EscapeSequences.Keys.ToArray()) != null) {
                raw     += stream.Char;
                escaped += EscapeSequences[stream.Char];
            }
            // \u h{4} or \U h{8}
            else if (stream.Eat("u", "U") != null) {
                var u = stream.Char;
                raw += u;
                var uEscapeLen = u == 'u' ? 4 : 8;
                var escapeLen = 0;
                while (escapeLen < uEscapeLen) {
                    if (stream.Eat(NumbersHex) != null) {
                        raw += stream.Char;
                        escapeLen++;
                    }
                    else {
                        MarkBlame(BlameType.TruncatedEscapeSequence);
                        return;
                    }
                }

                var num = Utilities.ParseInt(raw[2..], 16);
                if (num != null) {
                    const int unicodeUpperBound = 0x10ffff;
                    if (0 <= num && num <= unicodeUpperBound) {
                        escaped += num;
                    }
                    else {
                        MarkBlame(BlameType.IllegalUnicodeCharacter);
                        return;
                    }
                }
                else {
                    MarkBlame(BlameType.InvalidEscapeSequence);
                    return;
                }
            }
            else if (stream.Eat("x") != null) {
                raw += stream.Char;
                while (stream.Eat(NumbersHex) != null && raw.Length < 4) {
                    raw += stream.Char;
                }

                if (raw.Length != 4) {
                    MarkBlame(BlameType.InvalidXEscapeFormat);
                    return;
                }

                var num = Utilities.ParseInt(raw[2..], 16);
                if (num != null) {
                    escaped += num;
                }
                else {
                    MarkBlame(BlameType.InvalidEscapeSequence);
                    return;
                }
            }
            else if (stream.AtEndOfLine) {
                MarkBlame(BlameType.TruncatedEscapeSequence);
                return;
            }
            else {
                MarkBlame(BlameType.InvalidEscapeSequence);
                return;
            }

            void MarkBlame(BlameType blameType) {
                LanguageReport.To(
                    blameType,
                    new CodeSpan(unit, escapeStart, stream.Location - (0, 1))
                );
                value.Append(raw);
                content.Append(raw);
            }

            value.Append(raw);
            content.Append(escaped);
        }
    }
}
