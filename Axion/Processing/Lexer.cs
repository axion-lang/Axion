using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Axion.Tokens;

namespace Axion.Processing {
    /// <summary>
    ///     Static tool for splitting Axion code into tokens <see cref="LinkedList{T}" />.
    /// </summary>
    internal static class Lexer {
        /// <summary>
        /// 	Reference to current processing <see cref="SourceCode" /> instance.
        /// </summary>
        private static SourceCode src;

        /// <summary>
        ///     Resulting <see cref="LinkedList{T}" /> of tokens.
        ///     Reference is equal to chain specified in <see cref="Tokenize" /> method.
        /// </summary>
        private static LinkedList<Token> tokens;

        #region Input source values

        /// <summary>
        ///     Current processing code divided by lines.
        /// </summary>
        private static string[] lines;

        /// <summary>
        ///     Current line and column index in code. 0-based.
        /// </summary>
        private static (int line, int column) pos;

        /// <summary>
        ///     Current evaluating code line.
        /// </summary>
        private static string Line => lines[pos.line];

        /// <summary>
        ///     Current evaluating character in code.
        /// </summary>
        private static char C =>
            pos.line < lines.Length
                ? pos.column < Line.Length
                      ? Line[pos.column]
                      : Spec.EndLine
                : Spec.EndFile;

        #endregion

        #region Current token values

        /// <summary>
        ///     Type of current reading <see cref="Token" />.
        /// </summary>
        private static TokenType tokenType;

        /// <summary>
        ///     Value of current reading <see cref="Token" />
        /// </summary>
        private static readonly StringBuilder tokenValue = new StringBuilder();

        /// <summary>
        ///     Start position of current reading <see cref="Token" />.
        /// </summary>
        private static (int line, int column) tokenPos;

        #endregion

        #region Indentation properties

        /// <summary>
        ///     Last code line indentation length.
        /// </summary>
        private static int indentLevel;

        /// <summary>
        ///     Indentation character used in current code.
        /// </summary>
        private static char indentChar;

        /// <summary>
        ///     Returns <see langword="true" /> if code use mixed indentation
        ///     (partly by spaces, partly by tabs).
        /// </summary>
        private static bool inconsistentIndentation;

        #endregion

        /// <summary>
        ///     Contains unmatched unpaired parenthesis, brackets and braces.
        /// </summary>
        private static readonly List<Token> mismatchingPairs = new List<Token>();

        /// <summary>
        ///     Divides &lt;<see cref="SourceCode.Content" />&gt; into &lt;<see cref="SourceCode.Tokens"/>&gt; list of tokens.
        /// </summary>
        internal static void Tokenize(SourceCode source) {
            src    = source;
            lines  = src.Content;
            tokens = src.Tokens;

            Reset();

            while (pos.line < lines.Length && pos.column < Line.Length) {
                var token = ReadToken();
                if (token != null) {
                    tokens.AddLast(token);
                }
            }

            for (int i = 0; i < mismatchingPairs.Count; i++) {
                var mismatch = mismatchingPairs[i];
                switch (mismatch.Type) {
                    case TokenType.OpLeftParenthesis:
                    case TokenType.OpRightParenthesis: {
                        src.Errors.Add(
                            new ProcessingException(
                                ErrorType.MismatchedParenthesis,
                                src, mismatch
                            )
                        );
                        continue;
                    }
                    case TokenType.OpLeftBracket:
                    case TokenType.OpRightBracket: {
                        src.Errors.Add(
                            new ProcessingException(
                                ErrorType.MismatchedBracket,
                                src, mismatch
                            )
                        );
                        continue;
                    }
                    case TokenType.OpLeftBrace:
                    case TokenType.OpRightBrace: {
                        src.Errors.Add(
                            new ProcessingException(
                                ErrorType.MismatchedBrace,
                                src, mismatch
                            )
                        );
                        continue;
                    }
                    default: {
                        throw new Exception($"Internal error: {nameof(mismatchingPairs)} grabbed invalid {nameof(TokenType)}: {mismatch.Type}.");
                    }
                }
            }
        }

        /// <summary>
        /// 	Resets all <see cref="Lexer" /> values to work with next file.
        /// </summary>
        private static void Reset() {
            pos.line        = 0;
            pos.column      = 0;
            tokenPos.line   = 0;
            tokenPos.column = 0;
            indentLevel     = 0;
            indentChar      = '\0';
        }

        private static Token ReadToken() {
            // reset token properties
            tokenType = TokenType.Unknown;
            tokenPos  = pos;
            tokenValue.Clear();

            // end of file;
            if (C == Spec.EndFile) {
                tokenValue.Append(C);
                tokenType = TokenType.EndOfFile;
                Move();
            }

            // newline;
            else if (C == Spec.EndLine) {
                tokenValue.Append(C);
                tokenType = TokenType.Newline;
                Move();
                // don't add newline as first token
                // and don't create multiple newline tokens.
                if (tokens.Count == 0 || tokens.Last.Value.Type == TokenType.Newline) {
                    return null;
                }
            }

            // whitespaces & indentation;
            else if (C == ' ' || C == '\t') {
                // check if we're at line beginning
                if (tokens.Count != 0
                 && mismatchingPairs.Count == 0
                 && tokens.Last.Value.Type == TokenType.Newline) {
                    // compute indentation length
                    var indentLength = 0;

                    // set indent character if it is unknown
                    if (indentChar == '\0') {
                        indentChar = C;
                    }

                    while (C == ' ' || C == '\t') {
                        if (!inconsistentIndentation) {
                            // check for consistency
                            if (indentChar != C && !inconsistentIndentation) {
                                Logger.Warn(ErrorType.WarnInconsistentIndentation, pos);
                                inconsistentIndentation = true;
                            }
                        }

                        if (C == ' ') {
                            indentLength++;
                        }
                        else if (C == '\t') {
                            // tab size computation borrowed from python compiler
                            indentLength += 8 - indentLength % 8;
                        }
                        Move();
                    }

                    // skip indentation if line is commented
                    var restOfLine = Line.Substring(pos.column);
                    if ( // rest of line is blank
                        // next token is one-line comment
                        restOfLine.StartsWith(Spec.CommentOnelineStart.ToString())
                        // or next token is multiline comment
                     || (restOfLine.StartsWith(Spec.CommentMultilineStart) &&
                         // and comment goes through end of line
                         Regex.Matches(restOfLine, Spec.CommentMultilineStartPattern).Count >
                         Regex.Matches(restOfLine, Spec.CommentMultilineEndPattern).Count)) {
                        return null;
                    }

                    if (indentLength > indentLevel) {
                        // indent increased
                        tokenType = TokenType.Indent;
                    }
                    else if (indentLength < indentLevel) {
                        // indent decreased
                        tokenType = TokenType.Outdent;
                    }
                    indentLevel = indentLength;
                }
                // just whitespace
                else {
                    Move();
                    return null;
                }
            }

            // one-line comment;
            else if (C == Spec.CommentOnelineStart) {
                tokenValue.Append(Spec.CommentOnelineStart);
                tokenType = TokenType.CommentLiteral;
                Move();
                // skip all until end of line or end of file
                while (C != Spec.EndLine && C != Spec.EndFile) {
                    tokenValue.Append(C);
                    Move();
                }
            }

            // multiline comment;
            else if (C.ToString() + Peek() == Spec.CommentMultilineStart) {
                tokenValue.Append(Spec.CommentMultilineStart);
                tokenType = TokenType.CommentLiteral;
                Move(2);

                // multiline comment level variable.
                // FEATURE: support for nested multiline comments
                var commentLevel = 1;
                while (commentLevel > 0) {
                    var nextPiece = C.ToString() + Peek();
                    // comment end
                    if (nextPiece == Spec.CommentMultilineEnd) {
                        tokenValue.Append(Spec.CommentMultilineEnd);
                        Move(2);
                        // decrease comment level
                        commentLevel--;
                    }
                    // nested multiline comment start
                    else if (nextPiece == Spec.CommentMultilineStart) {
                        tokenValue.Append(Spec.CommentMultilineStart);
                        Move(2);
                        // increase comment level
                        commentLevel++;
                    }
                    // went through end of file
                    else if (C == Spec.EndFile) {
                        throw new ProcessingException(
                            ErrorType.UnclosedMultilineComment,
                            src,
                            new Token(TokenType.CommentLiteral, tokenPos, tokenValue.ToString())
                        );
                    }
                    // any other character
                    else {
                        tokenValue.Append(C);
                        Move();
                    }
                }
            }

            // number;
            else if (char.IsDigit(C)) {
                return ReadNumber();
            }

            // identifier;
            else if (Spec.IsValidIdStart(C)) {
                tokenValue.Append(C);
                Move();
                while (Spec.IsValidIdChar(C)) {
                    tokenValue.Append(C);
                    Move();
                }

                if (!Spec.Keywords.TryGetValue(tokenValue.ToString(), out tokenType)) {
                    tokenType = TokenType.Identifier;
                }
            }

            // operator;
            else if (Spec.OperatorChars.Contains(C)) {
                var longestLength = Spec.OperatorsValues[0].Length;
                var next          = C + Peek((uint) longestLength - 1u);
                for (var length = longestLength; length > 0; length--) {
                    var piece = next.Substring(0, length);
                    if (Spec.OperatorsValues.Contains(piece)) {
                        Move(length);
                        var op = new OperatorToken(piece, tokenPos);
                        // got open brace
                        if (op.Properties.IsOpenBrace) {
                            mismatchingPairs.Add(op);
                        }
                        else if (op.Properties.IsCloseBrace) {
                            // got mismatching close brace
                            if (mismatchingPairs.Count == 0) {
                                mismatchingPairs.Add(op);
                            }
                            // got matching close brace
                            else if (op.Properties.MatchingBrace == mismatchingPairs.Last().Type) {
                                mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                            }
                        }
                        return op;
                    }
                }
                src.Errors.Add(
                    new ProcessingException(
                        ErrorType.InvalidOperator,
                        src,
                        new OperatorToken(
                            new OperatorProperties(
                                TokenType.Unknown,
                                InputSide.Both,
                                Associativity.None,
                                false, 0
                            ),
                            tokenPos
                        )
                    )
                );
            }

            // string
            else if (Spec.StringQuotes.Contains(C) ||
                     Spec.StringPrefixes.Contains(C) &&
                     Spec.StringQuotes.Contains(Peek())) {
                tokenType = TokenType.StringLiteral;
                // TODO add string prefixes processing support
                //bool strFormat = false;
                if (C == 'f') {
                    //strFormat = true;
                    Move();
                }
                var delimiter = C.ToString();
                {
                    var firstQuote = C;
                    Move();
                    // add next 2 quotes for multiline strings
                    if (C == firstQuote) {
                        delimiter += C;
                        Move();
                        if (C != firstQuote) {
                            // got empty one-quoted string
                            return new Token(TokenType.StringLiteral, pos);
                        }
                        delimiter += C;
                        Move();
                    }
                }
                // TODO rewrite BUG string skips if piece long
                var nextPiece = new StringBuilder();
                while (true) {
                    var charBeforeDelimiter = C;
                    // get next piece of string
                    for (var i = 0; i < delimiter.Length; i++) {
                        nextPiece.Append(C);
                        Move();
                    }
                    // compare with non-escaped delimiter
                    if (nextPiece.ToString() == delimiter &&
                        charBeforeDelimiter != '\\') {
                        break;
                    }

                    // if not matched, check for end of line/file
                    if (C == Spec.EndLine && delimiter.Length == 1 ||
                        C == Spec.EndFile) {
                        throw new ProcessingException(
                            ErrorType.UnclosedString,
                            src,
                            new Token(TokenType.StringLiteral, tokenPos, tokenValue.ToString())
                        );
                    }
                    tokenValue.Append(nextPiece);
                    nextPiece.Clear();
                }
            }

            // character literal
            else if (C == Spec.CharLiteralQuote) {
                tokenType = TokenType.CharLiteral;
                Move();
                // got escape-sequence
                if (C == '\\') {
                    tokenValue.Append('\\');
                    Move();
                    // got invalid sequence
                    if (!Spec.ValidEscapeChars.Contains(C)) {
                        tokenValue.Append(C);
                        src.Errors.Add(
                            new ProcessingException(
                                ErrorType.InvalidEscapeSequence,
                                src,
                                new Token(TokenType.Invalid, tokenPos, tokenValue.ToString())
                            )
                        );
                    }
                }
                tokenValue.Append(C);
                Move();
                if (C != Spec.CharLiteralQuote) {
                    throw new ProcessingException(
                        ErrorType.CharacterLiteralTooLong,
                        src,
                        new Token(TokenType.Invalid, tokenPos, tokenValue.ToString())
                    );
                }
                Move();
            }

            // invalid
            else {
                src.Errors.Add(
                    new ProcessingException(
                        ErrorType.InvalidSymbol,
                        src,
                        new Token(TokenType.Unknown, pos, C.ToString())
                    )
                );
            }

            return new Token(tokenType, tokenPos, tokenValue.ToString());
        }

        private static Token ReadNumber() {
            var isPrefix0 = false;
            if (C == '0') {
                tokenValue.Append(C);
                Move();
                // second char
                if (C == 'b' || C == 'B') {
                    tokenValue.Append(C);
                    return ReadBinaryNumber();
                }
                if (C == 'o' || C == 'O') {
                    tokenValue.Append(C);
                    return ReadOctalNumber();
                }
                if (C == 'x' || C == 'X') {
                    tokenValue.Append(C);
                    return ReadHexNumber();
                }
                isPrefix0 = true;
                // skip leading zeros
                while (C == '0') {
                    tokenValue.Append('0');
                    Move();
                }
            }
            var isFirstChar = true;
            while (true) {
                switch (C) {
                    case '.': {
                        return ReadFraction();
                    }
                    case 'e':
                    case 'E': {
                        var exp = ReadExponent();
                        if (exp != null) {
                            return exp;
                        }
                        return new ConstToken(
                            TokenType.Unknown,
                            tokenPos,
                            ParseInteger(tokenValue.ToString(), 10)
                        );
                    }
                    case 'j':
                    case 'J': {
                        return new ConstToken(
                            TokenType.Unknown,
                            tokenPos,
                            LiteralParser.ParseImaginary(tokenValue.ToString())
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
                        tokenValue.Append(C);
                        break;
                    }
                    default: {
                        if (isPrefix0 && !isFirstChar) {
                            throw new ProcessingException(
                                ErrorType.InvalidIntegerLiteral,
                                src,
                                new Token(
                                    TokenType.Unknown,
                                    tokenPos,
                                    tokenValue.ToString()
                                )
                            );
                        }
                        return new ConstToken(
                            TokenType.Unknown,
                            tokenPos,
                            ParseInteger(tokenValue.ToString(), 10)
                        );
                    }
                }
                Move();
                isFirstChar = false;
            }
        }

        private static Token ReadBinaryNumber() {
            var bits      = 0;
            var iVal      = 0;
            var useBigInt = false;
            var bigInt    = BigInteger.Zero;
            var first     = true;
            while (true) {
                Move();
                tokenValue.Append(C);
                switch (C) {
                    case '0': {
                        if (iVal != 0) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    }
                    case '1': {
                        bits++;
                        if (bits == 32) {
                            useBigInt = true;
                            bigInt    = iVal;
                        }
                        if (bits >= 32) {
                            bigInt = (bigInt << 1) | (C - '0');
                        }
                        else {
                            iVal = (iVal << 1) | (C - '0');
                        }
                        break;
                    }
                    case 'l':
                    case 'L': {
                        var value = useBigInt ? bigInt : iVal;
                        return new ConstToken(TokenType.Unknown, tokenPos, value);
                    }
                    default: {
                        if (first) {
                            throw new ProcessingException(
                                ErrorType.InvalidBinaryLiteral,
                                src,
                                new Token(TokenType.Unknown, tokenPos, tokenValue.ToString())
                            );
                        }
                        var value = useBigInt ? bigInt : (object) iVal;
                        return new ConstToken(TokenType.Unknown, tokenPos, value);
                    }
                }
                first = false;
            }
        }

        private static Token ReadOctalNumber() {
            var first = true;
            while (true) {
                Move();
                tokenValue.Append(C);
                switch (C) {
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
                        var value = LiteralParser.ParseBigInteger(tokenValue.ToString().Substring(2, tokenValue.Length - 2), 8);
                        return new ConstToken(TokenType.Unknown, tokenPos, value);
                    }
                    default: {
                        if (first) {
                            throw new ProcessingException(
                                ErrorType.InvalidOctalLiteral,
                                src,
                                new Token(TokenType.Unknown, tokenPos, tokenValue.ToString())
                            );
                        }
                        var value = ParseInteger(tokenValue.ToString().Substring(2), 8);
                        return new ConstToken(TokenType.Unknown, tokenPos, value);
                    }
                }
                first = false;
            }
        }

        private static Token ReadHexNumber() {
            var first = true;
            while (true) {
                Move();
                tokenValue.Append(C);
                if (char.IsDigit(C)
                 || C == 'a' || C == 'b' || C == 'c' || C == 'd' || C == 'e' || C == 'f'
                 || C == 'A' || C == 'B' || C == 'C' || C == 'D' || C == 'E' || C == 'F') {
                }
                else if (C == 'l' || C == 'L') {
                    var value = LiteralParser
                        .ParseBigInteger(
                            tokenValue
                                .ToString()
                                .Substring(2, tokenValue.Length - 3), 16
                        );
                    return new ConstToken(TokenType.Unknown, tokenPos, value);
                }
                else {
                    if (first) {
                        throw new ProcessingException(
                            ErrorType.InvalidHexadecimalLiteral,
                            src,
                            new Token(TokenType.Unknown, tokenPos, tokenValue.ToString())
                        );
                    }
                    var value = ParseInteger(tokenValue.ToString().Substring(2), 16);
                    return new ConstToken(TokenType.Unknown, tokenPos, value);
                }
                first = false;
            }
        }

        private static Token ReadFraction() {
            while (true) {
                Move();
                tokenValue.Append(C);
                if (char.IsDigit(C)) {
                }
                else if (C == 'e' || C == 'E') {
                    var exp = ReadExponent();
                    if (exp != null) {
                        return exp;
                    }
                    var value = ParseFloat(tokenValue.ToString());
                    return new ConstToken(TokenType.Unknown, tokenPos, value);
                }
                else if (C == 'j' || C == 'J') {
                    var value = LiteralParser.ParseImaginary(tokenValue.ToString());
                    return new ConstToken(TokenType.Unknown, tokenPos, value);
                }
                else {
                    var value = ParseFloat(tokenValue.ToString());
                    return new ConstToken(TokenType.Unknown, tokenPos, value);
                }
            }
        }

        private static Token ReadExponent() {
            var startPos = pos;
            Move();
            tokenValue.Append(C);
            if (C == '-' || C == '+') {
                Move();
            }
            object value;
            while (true) {
                if (char.IsDigit(C)) {
                    tokenValue.Append(C);
                    Move();
                }
                else if (C == 'j' || C == 'J') {
                    value = LiteralParser.ParseImaginary(tokenValue.ToString());
                    break;
                }
                else {
                    if (startPos.column == pos.column) {
                        return null;
                    }
                    value = ParseFloat(tokenValue.ToString());
                    break;
                }
            }
            return new ConstToken(TokenType.Unknown, tokenPos, value);
        }

        private static object ParseInteger(string s, int radix) {
            try {
                return LiteralParser.ParseInteger(s, radix);
            }
            catch (ArgumentException ex) {
                throw new ProcessingException(
                    ex, ErrorType.InvalidIntegerLiteral,
                    Line, new Token(TokenType.Unknown, tokenPos, s)
                );
            }
        }

        private static object ParseFloat(string s) {
            try {
                return LiteralParser.ParseFloat(s);
            }
            catch (Exception ex) {
                throw new ProcessingException(
                    ex, ErrorType.InvalidFloatLiteral,
                    Line, new Token(TokenType.Unknown, tokenPos, s)
                );
            }
        }

        #region Source stream control functions

        private static char Peek() {
            // if last char was EOF
            // or line out of range
            if (C == Spec.EndFile
             || pos.line > lines.Length) {
                return Spec.EndFile;
            }
            // if column out of range
            if (pos.column + 1 >= Line.Length) {
                // range of line
                if (pos.line < lines.Length) {
                    return Spec.EndLine;
                }
                // range of last line
                return Spec.EndFile;
            }
            // else just return next char
            return Line[pos.column + 1];
        }

        private static string Peek(uint length) {
            // save values
            var savedPos         = pos;
            var savedIndentLevel = indentLevel;

            var value = "";
            for (var i = 0; i < length; i++) {
                Move();
                value += C;
            }
            // restore values
            pos         = savedPos;
            indentLevel = savedIndentLevel;
            return value;
        }

        /// <summary>
        ///     Moves <see cref="C" /> value by
        ///     &lt;<paramref name="position" />&gt; in code
        ///     and updates <see cref="pos" /> values.
        /// </summary>
        private static void Move(int position = 1) {
            if (position == 0) {
                return;
            }
            if (position > 0) {
                for (var i = 0; i < position; i++) {
                    if (C == Spec.EndLine) {
                        pos.line++;
                        pos.column = 0;
                    }
                    else {
                        pos.column++;
                    }
                }
            }
            else {
                for (var i = position; i > 0; i--) {
                    if (C == Spec.EndLine) {
                        pos.line--;
                        pos.column = lines[pos.line].Length - 2;
                    }
                    else {
                        pos.column--;
                    }
                }
            }
        }

        #endregion
    }
}