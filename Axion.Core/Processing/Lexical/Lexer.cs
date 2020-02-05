using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Source;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical {
    public class Lexer {
        private readonly SourceUnit src;
        private readonly TextStream stream;

        // Variables for current token creation
        private          TokenType     type;
        private readonly StringBuilder value   = new StringBuilder();
        private readonly StringBuilder content = new StringBuilder();
        private          Location      startLoc;

        // Variables for indentation analysis
        private char indentChar = '\0';
        private int  indentSize;
        private int  lastIndentLen;
        private int  indentLevel;

        public readonly Stack<Token>     MismatchingPairs   = new Stack<Token>();
        public readonly Stack<TokenType> ProcessTerminators = new Stack<TokenType>();

        public Lexer(SourceUnit source) {
            src    = source;
            stream = src.TextStream;
            ProcessTerminators.Push(End);
        }

        private bool AddChar(bool isContent = true, params char[] expect) {
            string eaten = stream.Eat(expect) ?? "";
            value.Append(eaten);
            if (isContent) {
                content.Append(eaten);
            }

            return eaten != "";
        }

        private bool AddNext(bool isContent = true, params string[] expect) {
            string eaten = stream.Eat(expect) ?? "";
            value.Append(eaten);
            if (isContent) {
                content.Append(eaten);
            }

            return eaten != "";
        }

        private T BindSpan<T>(T token) where T : Token {
            if (token == null) {
                return null;
            }

            token.MarkStart(startLoc);
            token.MarkEnd(stream.Location);
            return token;
        }

        private Token NewTokenFromContext() {
            return new Token(src, type, value.ToString(), content.ToString());
        }

        public Token Read() {
            startLoc = stream.Location;
            type     = Invalid;
            value.Clear();
            content.Clear();
            return BindSpan(ReadInternal());
        }

        private Token ReadInternal() {
            if (stream.PeekIs(White)) {
                return ReadWhite();
            }

            if (stream.PeekIs(IdStart)) {
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

            if (stream.PeekIs(Eoc)) {
                return ReadEoc();
            }

            // invalid character token
            AddNext();
            Token t = NewTokenFromContext();
            LangException.Report(BlameType.InvalidCharacter, t);
            return t;
        }

        private Token ReadWhite() {
            while (AddChar(expect: White)) { }

            if (src.TokenStream.Tokens.Count == 0) {
                lastIndentLen = value.Length;
                type          = Whitespace;
                return NewTokenFromContext();
            }

            string ln = stream.RestOfLine;
            // check that whitespace is not meaningful here
            if (!src.TokenStream.Tokens.Last().Is(Newline) || string.IsNullOrWhiteSpace(ln) ||
                ln.StartsWith(OneLineCommentMark)          || MismatchingPairs.Count > 0    ||
                src.TokenStream.Tokens[^2] is OperatorToken op &&
                op.Side == InputSide.Both || NotIndentRegex.IsMatch(ln)) {
                src.TokenStream.Tokens[^1].EndingWhite += value;
                return null;
            }

            // indentation processing
            if (indentChar == '\0') {
                indentChar = value[0];
            }

            var      consistent         = true;
            Location inconsistencyStart = default;
            var      newIndentLen       = 0;
            for (var i = 0; i < value.Length; i++) {
                char ch = value[i];
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
                LangException.Report(
                    BlameType.InconsistentIndentation,
                    new Span(src, inconsistencyStart, stream.Location.Add(0, -1))
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
                    src.TokenStream.Tokens.Add(NewTokenFromContext());
                }
            }
            else {
                src.TokenStream.Tokens[^1].EndingWhite += value;
            }

            lastIndentLen = newIndentLen;
            return null;
        }

        private Token ReadId() {
            do {
                AddNext();
            } while (stream.PeekIs(IdPart)
                  && (!stream.PeekIs(IdNotEnd)
                   || IdAfterNotEnd.Contains(stream.Peek(2)[1])));

            if (stream.PeekIs(StringQuotes)) {
                return ReadString();
            }

            if (Keywords.TryGetValue(value.ToString(), out type)) {
                return NewTokenFromContext();
            }

            if (OperatorsKeys.Contains(value.ToString())) {
                return new OperatorToken(src, value.ToString());
            }

            type = Identifier;
            value.Replace("-", "_");
            content.Replace("-", "_");

            return NewTokenFromContext();
        }

        private Token ReadNewline() {
            type = Newline;
            while (AddChar(expect: Eols)) { }

            src.TokenStream.Tokens.Add(NewTokenFromContext());

            if (!src.TextStream.PeekIs(White)) {
                // root-level newline - reset indentation
                lastIndentLen = 0;
                while (indentLevel > 0) {
                    type     = Outdent;
                    startLoc = stream.Location;
                    src.TokenStream.Tokens.Add(NewTokenFromContext());
                    indentLevel--;
                }
            }

            return null;
        }

        private Token ReadEoc() {
            type = End;
            AddChar(expect: Eoc);
            return NewTokenFromContext();
        }

        private NumberToken ReadNumber() {
            while (AddChar(expect: '0')) { }

            while (AddChar(expect: NumbersDec)) {
                content.Append(stream.C);
            }

            return new NumberToken(src, value.ToString(), content.ToString());
        }

        private OperatorToken ReadOperator() {
            AddNext(expect: OperatorsKeys);
            return new OperatorToken(src, value.ToString());
        }

        private Token ReadPunctuation() {
            AddNext(expect: PunctuationKeys);
            type = Punctuation[value.ToString()];
            Token t = NewTokenFromContext();
            if (type.IsOpenBracket()) {
                MismatchingPairs.Push(t);
            }
            else if (type.IsCloseBracket()) {
                if (MismatchingPairs.Count                             > 0
                 && MismatchingPairs.First().Type.GetMatchingBracket() == type) {
                    MismatchingPairs.Pop();
                }
                else {
                    MismatchingPairs.Push(t);
                }
            }

            return t;
        }

        private CharToken ReadChar() {
            AddNext(false, CharacterQuote);
            while (!AddNext(false, CharacterQuote)) {
                if (stream.AtEndOfLine) {
                    var tu = new CharToken(src, value.ToString(), content.ToString(), true);
                    LangException.Report(BlameType.UnclosedCharacterLiteral, tu);
                    return tu;
                }

                if (stream.PeekIs('\\')) {
                    ReadEscapeSeq();
                }
                else {
                    AddNext();
                }
            }

            var t = new CharToken(src, value.ToString(), content.ToString());

            if (content.Length == 0) {
                LangException.Report(BlameType.EmptyCharacterLiteral, t);
            }
            else if (content.Replace("\\", "").Length != 1) {
                LangException.Report(BlameType.CharacterLiteralTooLong, t);
            }

            return t;
        }

        private CommentToken ReadOneLineComment() {
            AddNext(false, OneLineCommentMark);
            while (!stream.AtEndOfLine) {
                AddNext();
            }

            return new CommentToken(src, value.ToString(), content.ToString());
        }

        private CommentToken ReadMultiLineComment() {
            AddNext(false, MultiLineCommentMark);
            while (!stream.PeekIs(MultiLineCommentMark)) {
                if (stream.PeekIs(Eoc)) {
                    var t = new CommentToken(src, value.ToString(), content.ToString(), true, true);
                    LangException.Report(BlameType.UnclosedMultilineComment, t);
                    return t;
                }

                AddNext();
            }

            AddNext(false, MultiLineCommentMark);
            return new CommentToken(src, value.ToString(), content.ToString(), true);
        }

        private Token ReadString() {
            var prefixes = "";
            if (!string.IsNullOrWhiteSpace(value.ToString())) {
                // if string has prefixes, then
                // ReadString was called from ReadId,
                // and value == string prefixes.
                prefixes = value.ToString().ToLower();
                value.Clear();
                content.Clear();
                for (var i = 0; i < prefixes.Length; i++) {
                    char p  = prefixes[i];
                    var  ps = p.ToString();
                    if (!StringPrefixes.Contains(p)) {
                        LangException.Report(
                            BlameType.InvalidStringPrefix,
                            BindSpan(new Token(src, Invalid, ps, ps, "", stream.Location.Add(0, i)))
                        );
                    }
                }
            }

            AddChar(false, StringQuotes);
            var    quote        = value[0].ToString();
            string closingQuote = quote;
            if (AddNext(false, quote.Multiply(2))) {
                quote = quote.Multiply(3);
            }
            else if (AddNext(false, quote)) {
                var se = new StringToken(src, value.ToString(), content.ToString(), false, prefixes, quote);
                if (prefixes.Length > 0) {
                    LangException.Report(BlameType.RedundantPrefixesForEmptyString, se);
                }

                return se;
            }

            var interpolations = new List<StringInterpolation>();
            var unclosed       = false;
            while (true) {
                if (stream.PeekIs('\\') && !prefixes.Contains("r")) {
                    ReadEscapeSeq();
                }
                else if (stream.PeekIs(Eols) && quote.Length == 1
                      || stream.PeekIs(Eoc)) {
                    unclosed = true;
                    break;
                }
                else if (quote.Length == 1 && stream.PeekIs(closingQuote)) {
                    AddNext(false, closingQuote);
                    break;
                }
                else if (AddNext(false, closingQuote.Multiply(3))) {
                    break;
                }
                else if (prefixes.Contains("f") && stream.PeekIs("{")) {
                    interpolations.Add(ReadStringInterpolation());
                }
                else {
                    AddNext();
                }
            }

            var s = new StringToken(
                src,
                value.ToString(),
                content.ToString(),
                unclosed,
                prefixes,
                quote,
                interpolations
            );
            if (prefixes.Contains("f") && interpolations.Count == 0) {
                LangException.Report(BlameType.RedundantStringFormat, s);
            }

            if (unclosed) {
                LangException.Report(BlameType.UnclosedString, s);
            }

            return s;
        }

        private StringInterpolation ReadStringInterpolation() {
            var        interpolation = new StringInterpolation(stream);
            SourceUnit iSrc          = interpolation.Source;
            var        lexer         = new Lexer(interpolation.Source);

            lexer.ProcessTerminators.Push(CloseBrace);
            while (true) {
                Token token = lexer.Read();
                if (token == null) {
                    continue;
                }

                iSrc.TokenStream.Tokens.Add(token);
                if (lexer.ProcessTerminators.Contains(token.Type)) {
                    break;
                }
            }

            foreach (Token mismatch in lexer.MismatchingPairs) {
                LangException.Report(BlameType.MismatchedBracket, mismatch);
            }

            // remove '{' '}'
            iSrc.TokenStream.Tokens.RemoveAt(0);
            iSrc.TokenStream.Tokens.RemoveAt(
                iSrc.TokenStream.Tokens.Count - 1
            );
            return interpolation;
        }

        private void ReadEscapeSeq() {
            Location escapeStart = stream.Location;
            string   raw         = stream.Eat('\\');
            var      escaped     = "";
            // \t, \n, etc
            if (stream.Eat(EscapeSequences.Keys.ToArray()) != null) {
                raw     += stream.C;
                escaped += EscapeSequences[stream.C];
            }
            // \u h{4} of \U h{8}
            else if (stream.Eat("u", "U") != null) {
                string u = stream.C;
                raw += u;
                int uEscapeLen = u == "u" ? 4 : 8;
                var escapeLen  = 0;
                while (escapeLen < uEscapeLen) {
                    if (stream.Eat(NumbersHex) != null) {
                        raw += stream.C;
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
            else if (stream.Eat("x") != null) {
                raw += stream.C;
                while (stream.Eat(NumbersHex) != null && raw.Length < 4) {
                    raw += stream.C;
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
            else if (stream.AtEndOfLine) {
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
                    new Span(src, escapeStart, stream.Location.Add(0, -1))
                );
                value.Append(raw);
                content.Append(raw);
            }

            value.Append(raw);
            content.Append(escaped);
        }
    }
}