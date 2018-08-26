using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Static tool for splitting Axion code into tokens <see cref="LinkedList{T}" />.
    /// </summary>
    internal class Lexer { // TODO Lexer correct newlines; numbers parsing; string formatting & prefixes.
        /// <summary>
        ///     Reference to outgoing <see cref="LinkedList{T}" /> of tokens.
        /// </summary>
        private readonly LinkedList<Token> tokens = new LinkedList<Token>();

        /// <summary>
        ///     Contains all errors that raised during lexical analysis.
        /// </summary>
        private readonly List<SyntaxError> errors   = new List<SyntaxError>();

        /// <summary>
        ///     Contains all warnings that found during lexical analysis.
        /// </summary>
        private readonly List<SyntaxError> warnings = new List<SyntaxError>();

        private readonly SourceProcessingOptions options;

        /// <summary>
        ///     Reference to current processing code divided by lines.
        /// </summary>
        private readonly string code;

        #region Input source properties

        /// <summary>
        ///     Current line and column index in code. 0-based.
        /// </summary>
        private (int line, int column) pos = (0, 0);

        /// <summary>
        ///     Index of current evaluating character in code.
        /// </summary>
        private int cIndex;

        /// <summary>
        ///     Current evaluating character in code.
        /// </summary>
        private char c => code[cIndex];

        #endregion

        #region Current token properties

        /// <summary>
        ///     Value of current reading <see cref="Token" />
        /// </summary>
        private string tokenValue;

        /// <summary>
        ///     Start position of current reading <see cref="Token" />.
        /// </summary>
        private (int line, int column) tokenPos = (0, 0);

        #endregion

        #region Indentation properties

        /// <summary>
        ///     Last code line indentation length.
        /// </summary>
        private int indentLevel;

        /// <summary>
        ///     Indentation character used in current code.
        /// </summary>
        private char indentChar;

        /// <summary>
        ///     Returns <see langword="true" /> if code use mixed indentation
        ///     (partly by spaces, partly by tabs).
        /// </summary>
        private bool inconsistentIndentation;

        #endregion

        /// <summary>
        ///     Contains unpaired parenthesis, brackets and braces.
        /// </summary>
        private readonly List<Token> mismatchingPairs = new List<Token>();

        public Lexer(
            string                  code,
            out LinkedList<Token>   tokens,
            out List<SyntaxError>   errors,
            out List<SyntaxError>   warnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) {
            this.code = code.Replace("\r", "") + Spec.EndStream;
            tokens    = this.tokens;
            errors    = this.errors;
            warnings  = this.warnings;
            options   = processingOptions;
        }

        /// <summary>
        ///     Divides code into list of tokens.
        /// </summary>
        internal void Process() {
            Token token = ReadToken(out ErrorType occurredErrorType);
            while (true) {
                if (token != null) {
                    tokens.AddLast(token);
                    if (token.Type == TokenType.EndOfStream) {
                        break;
                    }
                    if (token is OperatorToken op) {
                        // got open brace
                        if (op.Properties.IsOpenBrace) {
                            mismatchingPairs.Add(tokens.Last.Value);
                        }
                        else if (op.Properties.IsCloseBrace) {
                            // got mismatching close brace
                            if (mismatchingPairs.Count == 0) {
                                mismatchingPairs.Add(tokens.Last.Value);
                            }
                            // got matching close brace
                            else if (op.Properties.MatchingBrace == mismatchingPairs.Last().Type) {
                                mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                            }
                        }
                    }
                }
                if (occurredErrorType != ErrorType.None) {
                    errors.Add(
                        new SyntaxError(
                            occurredErrorType,
                            code,
                            tokens.Last.Value
                        )
                    );
                }
                token = ReadToken(out occurredErrorType);
            }

            foreach (Token mismatch in mismatchingPairs) {
                ErrorType errorType;
                switch (mismatch.Type) {
                    case TokenType.OpLeftParenthesis:
                    case TokenType.OpRightParenthesis: {
                        errorType = ErrorType.MismatchedParenthesis;
                        break;
                    }
                    case TokenType.OpLeftBracket:
                    case TokenType.OpRightBracket: {
                        errorType = ErrorType.MismatchedBracket;
                        break;
                    }
                    case TokenType.OpLeftBrace:
                    case TokenType.OpRightBrace: {
                        errorType = ErrorType.MismatchedBrace;
                        break;
                    }
                    default: {
                        throw new Exception(
                            $"Internal error: {nameof(mismatchingPairs)} grabbed invalid {nameof(TokenType)}: {mismatch.Type}."
                        );
                    }
                }
                errors.Add(new SyntaxError(errorType, code, mismatch));
            }
        }

        private Token ReadToken(out ErrorType occurredErrorType) {
            // reset token properties
            tokenPos          = pos;
            tokenValue        = "";
            occurredErrorType = ErrorType.None;

            // newline
            if (c == '\r') {
                MoveNext();
                return null;
            }
            if (c == Spec.EndLine) {
                pos.line++;
                pos.column = 0;
                cIndex++;
                // don't add newline as first token
                // and don't create multiple newline tokens.
                if (tokens.Count == 0 || tokens.Last.Value.Type == TokenType.Newline) {
                    return null;
                }
                return new EndOfLineToken(tokenPos);
            }

            if (c == Spec.EndStream) {
                return new EndOfStreamToken(tokenPos);
            }

            // whitespaces & indentation
            if (c == ' ' || c == '\t') {
                // check if we're at line beginning
                if (tokens.Count != 0
                    && mismatchingPairs.Count == 0
                    && tokens.Last.Value.Type == TokenType.Newline) {
                    // set indent character if it is unknown
                    if (indentChar == '\0') {
                        indentChar = c;
                    }

                    // compute indentation length
                    var indentLength = 0;
                    while (c == ' ' || c == '\t') {
                        tokenValue += c;

                        // check for consistency
                        if (indentChar != c && !inconsistentIndentation) {
                            inconsistentIndentation = true;
                        }

                        if (c == ' ') {
                            indentLength++;
                        }
                        else if (c == '\t') {
                            // tab size computation borrowed from python compiler
                            indentLength += 8 - indentLength % 8;
                        }
                        MoveNext();
                    }

                    // return if line is empty / commented
                    {
                        string restOfLine = GetRestOfLine();
                        if (// rest of line is blank
                            string.IsNullOrWhiteSpace(restOfLine)
                            // rest is one-line comment
                            || restOfLine.StartsWith(Spec.CommentOnelineStart)
                            // or rest is multiline comment
                            || restOfLine.StartsWith(Spec.CommentMultilineStart)
                            // and comment goes through end of line
                            && Regex.Matches(restOfLine, Spec.CommentMultilineStartPattern).Count >
                            Regex.Matches(restOfLine, Spec.CommentMultilineEndPattern).Count) {
                            return null;
                        }
                    }

                    Token indentationToken = null;
                    if (indentLength > indentLevel) {
                        // indent increased
                        indentLevel      = indentLength;
                        indentationToken = new IndentToken(tokenPos, tokenValue);
                    }
                    else if (indentLength < indentLevel) {
                        // indent decreased
                        indentLevel      = indentLength;
                        indentationToken = new OutdentToken(tokenPos, tokenValue);
                    }
                    // warn user about inconsistency
                    if (indentationToken != null
                        && inconsistentIndentation
                        && options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                        warnings.Add(
                            new SyntaxError(
                                ErrorType.WarnInconsistentIndentation,
                                code,
                                indentationToken
                            )
                        );
                    }
                    // return if indent level not changed
                    return indentationToken;
                }
                if (options.HasFlag(SourceProcessingOptions.PreserveWhitespaces)) {
                    char whitespace = c;
                    MoveNext();
                    if (tokens.Count > 0 &&
                        tokens.Last.Value.Type == TokenType.Whitespace) {
                        tokens.Last.Value.Value += whitespace;
                        return null;
                    }
                    return new Token(TokenType.Whitespace, pos, whitespace.ToString());
                }
                // usefulness whitespace
                MoveNext();
                return null;
            }

            // one-line comment
            if (c.ToString() == Spec.CommentOnelineStart) {
                MoveNext();

                // skip all until end of line or end of file
                while (!AtEndOfLine() && c != Spec.EndStream) {
                    tokenValue += c;
                    MoveNext();
                }
                return new OneLineCommentToken(tokenPos, tokenValue);
            }

            // multiline comment
            if (c.ToString() + Peek() == Spec.CommentMultilineStart) {
                MoveNext(2);
                var commentLevel = 1;
                while (commentLevel > 0) {
                    string nextPiece = c.ToString() + Peek();
                    // found comment end
                    if (nextPiece == Spec.CommentMultilineEnd) {
                        // don't add last comment '*/'
                        if (commentLevel != 1) {
                            tokenValue += Spec.CommentMultilineEnd;
                        }
                        MoveNext(2);
                        // decrease comment level
                        commentLevel--;
                    }
                    // found nested multiline comment start
                    else if (nextPiece == Spec.CommentMultilineStart) {
                        tokenValue += Spec.CommentMultilineStart;
                        MoveNext(2);
                        // increase comment level
                        commentLevel++;
                    }
                    // went through end of file
                    else if (c == Spec.EndStream) {
                        occurredErrorType = ErrorType.UnclosedMultilineComment;
                        return new MultilineCommentToken(tokenPos, tokenValue);
                    }
                    // found any other character
                    else {
                        tokenValue += c;
                        MoveNext();
                    }
                }
                return new MultilineCommentToken(tokenPos, tokenValue);
            }

            // character literal
            if (c == Spec.CharLiteralQuote) {
                MoveNext();
                // got escape-sequence
                if (c == '\\') {
                    tokenValue += '\\';
                    MoveNext();
                    // got invalid sequence
                    if (!Spec.ValidEscapeChars.Contains(c)) {
                        tokenValue        += c;
                        occurredErrorType =  ErrorType.InvalidEscapeSequence;
                        return new CharacterToken(tokenPos, tokenValue);
                    }
                }
                else if (c == Spec.CharLiteralQuote) {
                    tokenValue += c;
                    MoveNext();
                    occurredErrorType = ErrorType.EmptyCharacterLiteral;
                    return new CharacterToken(tokenPos, tokenValue);
                }
                tokenValue += c;
                MoveNext();
                if (c != Spec.CharLiteralQuote) {
                    occurredErrorType = ErrorType.CharacterLiteralTooLong;
                    return new CharacterToken(tokenPos, "");
                }
                MoveNext();
                return new CharacterToken(tokenPos, tokenValue);
            }

            // number
            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            // string
            if (Spec.StringQuotes.Contains(c) ||
                Spec.StringPrefixes.Contains(c) &&
                Spec.StringQuotes.Contains(Peek())) {
                // TODO add string prefixes processing support
                var stringOptions = StringLiteralOptions.None;
                if (c == 'f') {
                    stringOptions |= StringLiteralOptions.Format;
                    MoveNext();
                }
                if (c == 'r') {
                    stringOptions |= StringLiteralOptions.Raw;
                    MoveNext();
                }
                string delimiter = c.ToString();
                char   quote     = c;
                MoveNext();
                // add next 2 quotes for multiline strings
                if (c == quote) {
                    delimiter += c;
                    MoveNext();
                    if (c != quote) {
                        // got empty one-quoted string
                        return new StringToken(
                            tokenPos,
                            "",
                            quote,
                            stringOptions,
                            false
                        );
                    }
                    delimiter += c;
                    MoveNext();
                }

                while (true) {
                    string piece = c.ToString();
                    // TODO add string escape sequences
                    // if got non-escaped quote
                    if (c == quote && (tokenValue.Length == 0 || tokenValue[tokenValue.Length - 1] != '\\')) {
                        MoveNext();
                        piece += c;
                        // "
                        if (delimiter.Length == 1) {
                            return new StringToken(
                                tokenPos,
                                tokenValue,
                                quote,
                                stringOptions,
                                false
                            );
                        }
                        // """
                        if (c == quote && Peek() == quote) {
                            MoveNext(2);
                            return new StringToken(
                                tokenPos,
                                tokenValue,
                                quote,
                                stringOptions,
                                true
                            );
                        }
                        MoveNext();
                        piece += c;
                    }

                    // if not matched, check for end of line/file
                    if (AtEndOfLine() && delimiter.Length == 1 ||
                        c == Spec.EndStream) {
                        occurredErrorType = ErrorType.UnclosedString;
                        return new StringToken(
                            tokenPos,
                            tokenValue,
                            quote,
                            stringOptions,
                            delimiter.Length == 3
                        );
                    }
                    tokenValue += piece;
                    MoveNext();
                }
            }

            // identifier
            if (Spec.IsValidIdStart(c)) {
                tokenValue += c;
                MoveNext();
                while (Spec.IsValidIdChar(c)) {
                    tokenValue += c;
                    MoveNext();
                }

                if (!Spec.Keywords.TryGetValue(tokenValue, out TokenType tokenType)) {
                    return new IdentifierToken(tokenPos, tokenValue);
                }
                return new KeywordToken(tokenPos, tokenType, tokenValue);
            }

            // operator
            if (Spec.OperatorChars.Contains(c)) {
                int    longestLength = Spec.OperatorsValues[0].Length;
                string next          = c + Peek((uint) longestLength - 1u);
                for (int length = longestLength; length > 0; length--) {
                    string piece = next.Substring(0, length);
                    if (Spec.OperatorsValues.Contains(piece)) {
                        MoveNext(length);
                        return new OperatorToken(piece, tokenPos);
                    }
                }
                occurredErrorType = ErrorType.InvalidOperator;
                return new OperatorToken(Spec.InvalidOperatorProperties, tokenPos);
            }

            // invalid
            occurredErrorType = ErrorType.InvalidSymbol;
            MoveNext();
            return new Token(TokenType.Unknown, pos, c.ToString());
        }

        private Token ReadNumber() {
            var isPrefix0 = false;
            if (c == '0') {
                tokenValue += c;
                MoveNext();
                // second char
                if (c == 'b' || c == 'B') {
                    tokenValue += c;
                    return ReadBinaryNumber();
                }
                if (c == 'o' || c == 'O') {
                    tokenValue += c;
                    return ReadOctalNumber();
                }
                if (c == 'x' || c == 'X') {
                    tokenValue += c;
                    return ReadHexNumber();
                }
                isPrefix0 = true;
                // skip leading zeros
                while (c == '0') {
                    tokenValue += '0';
                    MoveNext();
                }
            }
            var isFirstChar = true;
            while (true) {
                switch (c) {
                    case '.': {
                        return ReadFraction();
                    }
                    case 'e':
                    case 'E': {
                        Token exp = ReadExponent();
                        if (exp != null) {
                            return exp;
                        }
                        return new Token(
                            TokenType.Unknown,
                            tokenPos,
                            ParseInteger(tokenValue, 10).ToString()
                        );
                    }
                    case 'j':
                    case 'J': {
                        return new Token(
                            TokenType.Unknown,
                            tokenPos,
                            LiteralParser.ParseImaginary(tokenValue).ToString()
                        );
                    }
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9': {
                        tokenValue += c;
                        break;
                    }
                    default: {
                        if (isPrefix0 && !isFirstChar) {
                            var errorToken = new Token(
                                TokenType.Unknown,
                                tokenPos,
                                tokenValue
                            );
                            errors.Add(
                                new SyntaxError(
                                    ErrorType.InvalidIntegerLiteral,
                                    code,
                                    errorToken
                                )
                            );
                            return errorToken;
                        }
                        return new Token(
                            TokenType.Unknown,
                            tokenPos,
                            ParseInteger(tokenValue, 10).ToString()
                        );
                    }
                }
                MoveNext();
                isFirstChar = false;
            }
        }

        private Token ReadBinaryNumber() {
            var        bits       = 0;
            var        longValue  = 0L;
            var        numOptions = NumberOptions.Bit8;
            BigInteger bigInt     = BigInteger.Zero;
            var        first      = true;
            while (true) {
                MoveNext();
                tokenValue += c;
                switch (c) {
                    case '0': {
                        if (longValue != 0L) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    }
                    case '1': {
                        bits++;
                        if (bits == 8) {
                            numOptions |= NumberOptions.Bit16;
                        }
                        else if (bits == 16) {
                            numOptions |= NumberOptions.Bit32;
                        }
                        else if (bits == 32) {
                            numOptions |= NumberOptions.Bit64;
                        }
                        else if (bits == 64) {
                            numOptions |= NumberOptions.BitNoLimit;
                            bigInt     =  longValue;
                        }
                        // TODO debug
                        var or = (byte) (c - '0');
                        if (bits >= 64) {
                            bigInt = (bigInt << 1) | or;
                        }
                        else {
                            longValue = (longValue << 1) | or;
                        }
                        break;
                    }
                    case 'l':
                    case 'L': {
                        BigInteger value = numOptions.HasFlag(NumberOptions.BitNoLimit) ? bigInt : longValue;
                        return new NumberToken(tokenPos, value, numOptions);
                    }
                    default: {
                        if (first) {
                            var errorToken = new Token(TokenType.Invalid, tokenPos, tokenValue);
                            errors.Add(
                                new SyntaxError(
                                    ErrorType.InvalidBinaryLiteral,
                                    code,
                                    errorToken
                                )
                            );
                            return errorToken;
                        }
                        object value = numOptions.HasFlag(NumberOptions.BitNoLimit) ? bigInt : (object) longValue;
                        return new NumberToken(tokenPos, value, numOptions);
                    }
                }
                first = false;
            }
        }

        private Token ReadOctalNumber() {
            var first = true;
            while (true) {
                MoveNext();
                tokenValue += c;
                switch (c) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7': {
                        break;
                    }
                    case 'l':
                    case 'L': {
                        BigInteger value = LiteralParser.ParseBigInteger(
                            tokenValue.Substring(2, tokenValue.Length - 2), 8
                        );
                        return new Token(TokenType.Unknown, tokenPos, value.ToString());
                    }
                    default: {
                        if (first) {
                            var errorToken = new Token(TokenType.Invalid, tokenPos, tokenValue);
                            errors.Add(
                                new SyntaxError(
                                    ErrorType.InvalidOctalLiteral,
                                    code,
                                    errorToken
                                )
                            );
                            return errorToken;
                        }
                        object value = ParseInteger(tokenValue.Substring(2), 8);
                        return new Token(TokenType.Unknown, tokenPos, value.ToString());
                    }
                }
                first = false;
            }
        }

        private Token ReadHexNumber() {
            var first = true;
            while (true) {
                MoveNext();
                tokenValue += c;
                if (char.IsDigit(c)
                 || c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f'
                 || c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F') {
                }
                else if (c == 'l' || c == 'L') {
                    BigInteger value = LiteralParser
                        .ParseBigInteger(
                            tokenValue
                                .Substring(2, tokenValue.Length - 3), 16
                        );
                    return new Token(TokenType.Unknown, tokenPos, value.ToString());
                }
                else {
                    if (first) {
                        var errorToken = new Token(TokenType.Invalid, tokenPos, tokenValue);
                        errors.Add(
                            new SyntaxError(
                                ErrorType.InvalidHexadecimalLiteral,
                                code,
                                errorToken
                            )
                        );
                        return errorToken;
                    }
                    object value = ParseInteger(tokenValue.Substring(2), 16);
                    return new Token(TokenType.Unknown, tokenPos, value.ToString());
                }
                first = false;
            }
        }

        private Token ReadFraction() {
            while (true) {
                MoveNext();
                tokenValue += c;
                if (char.IsDigit(c)) {
                }
                else if (c == 'e' || c == 'E') {
                    Token exp = ReadExponent();
                    if (exp != null) {
                        return exp;
                    }
                    object value = ParseFloat(tokenValue);
                    return new Token(TokenType.Unknown, tokenPos, value.ToString());
                }
                else if (c == 'j' || c == 'J') {
                    Complex value = LiteralParser.ParseImaginary(tokenValue);
                    return new Token(TokenType.Unknown, tokenPos, value.ToString());
                }
                else {
                    object value = ParseFloat(tokenValue);
                    return new Token(TokenType.Unknown, tokenPos, value.ToString());
                }
            }
        }

        private Token ReadExponent() {
            (int line, int column) startPos = pos;
            MoveNext();
            tokenValue += c;
            if (c == '-' || c == '+') {
                MoveNext();
            }
            object value;
            while (true) {
                if (char.IsDigit(c)) {
                    tokenValue += c;
                    MoveNext();
                }
                else if (c == 'j' || c == 'J') {
                    value = LiteralParser.ParseImaginary(tokenValue);
                    break;
                }
                else {
                    if (startPos.column == pos.column) {
                        return null;
                    }
                    value = ParseFloat(tokenValue);
                    break;
                }
            }
            return new Token(TokenType.Unknown, tokenPos, value.ToString());
        }

        private object ParseInteger(string s, int radix) {
            try {
                return LiteralParser.ParseInteger(s, radix);
            }
            catch (ArgumentException ex) {
                throw;

                //new ProcessingException(
                //    ex, ErrorType.InvalidIntegerLiteral,
                //    Line, new Token(TokenType.Invalid, tokenPos, s)
                //);
            }
        }

        private object ParseFloat(string s) {
            try {
                return LiteralParser.ParseFloat(s);
            }
            catch (Exception ex) {
                throw;

                //    new ProcessingException(
                //    ex, ErrorType.InvalidFloatLiteral,
                //    Line, new Token(TokenType.Invalid, tokenPos, s)
                //);
            }
        }

        #region Source stream control functions

        private bool AtEndOfLine() {
            return c == '\r' || c == Spec.EndLine;
        }

        private string GetRestOfLine() {
            int indexOfNewline = code.Substring(cIndex).IndexOf(Spec.EndLine);
            if (indexOfNewline == -1) {
                return "";
            }
            return code.Substring(cIndex, indexOfNewline);
        }

        private char Peek() {
            if (cIndex + 1 < code.Length) {
                return code[cIndex + 1];
            }
            return Spec.EndStream;
        }

        private string Peek(uint length) {
            // save values
            (int, int) savedPos         = pos;
            int        savedCIndex      = cIndex;
            int        savedIndentLevel = indentLevel;

            var value = "";
            for (var i = 0; i < length; i++) {
                MoveNext();
                value += c;
            }
            // restore values
            pos         = savedPos;
            cIndex      = savedCIndex;
            indentLevel = savedIndentLevel;
            return value;
        }

        /// <summary>
        ///     Moves <see cref="pos" /> values by
        ///     &lt;<paramref name="position" />&gt;.
        /// </summary>
        private void MoveNext(int position = 1) {
            if (position <= 0) {
                throw new Exception(
                    "Internal error: function " + nameof(MoveNext) + " was called with " + nameof(position) + " <= 0."
                );
            }

            for (var i = 0; i < position; i++) {
                // check for stream end
                if (c == Spec.EndStream) {
                    return;
                }
                pos.column++;
                cIndex++;
            }
        }

        #endregion
    }
}