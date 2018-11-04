using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Static tool for splitting Axion code into tokens <see cref="LinkedList{T}" />.
    /// </summary>
    internal class Lexer {
        /// <summary>
        ///     Reference to outgoing <see cref="LinkedList{T}" /> of tokens.
        /// </summary>
        private readonly LinkedList<Token> tokens;

        /// <summary>
        ///     All errors that found during lexical analysis.
        /// </summary>
        private readonly List<SyntaxError> errors;

        /// <summary>
        ///     All warnings that found during lexical analysis.
        /// </summary>
        private readonly List<SyntaxError> warnings;

        /// <summary>
        ///     Lexical analysis options enum.
        /// </summary>
        private SourceProcessingOptions options;

        private readonly string[] terminatorValues;

        private CharStream stream;

        private char c => stream.c;

        #region Current token properties

        /// <summary>
        ///     Value of current reading <see cref="Token" />
        /// </summary>
        private string tokenValue;

        /// <summary>
        ///     Start position of current reading <see cref="Token" />.
        /// </summary>
        private (int line, int column) tokenStartPosition = (0, 0);

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
            string                  codeToProcess,
            LinkedList<Token>       outTokens,
            List<SyntaxError>       outErrors,
            List<SyntaxError>       outWarnings,
            SourceProcessingOptions processingOptions     = SourceProcessingOptions.None,
            string[]                processingTerminators = null
        ) {
            if (!codeToProcess.EndsWith(Spec.EndOfStream.ToString())) {
                codeToProcess += Spec.EndOfStream;
            }
            stream           = new CharStream(codeToProcess);
            tokens           = outTokens ?? new LinkedList<Token>();
            errors           = outErrors ?? new List<SyntaxError>();
            warnings         = outWarnings ?? new List<SyntaxError>();
            options          = processingOptions;
            terminatorValues = processingTerminators;
        }

        private Lexer(
            CharStream              fromStream,
            LinkedList<Token>       outTokens,
            List<SyntaxError>       outErrors,
            List<SyntaxError>       outWarnings,
            SourceProcessingOptions processingOptions,
            string[]                processingTerminators
        ) {
            stream           = new CharStream(fromStream);
            tokens           = outTokens ?? new LinkedList<Token>();
            errors           = outErrors ?? new List<SyntaxError>();
            warnings         = outWarnings ?? new List<SyntaxError>();
            options          = processingOptions;
            terminatorValues = processingTerminators;
        }

        /// <summary>
        ///     Divides code into list of tokens.
        /// </summary>
        internal void Process() {
            Token token = NextToken();
            while (true) {
                if (token != null) {
                    tokens.AddLast(token);
                    // check for processing terminator
                    if (token.Type == TokenType.EndOfStream
                     || terminatorValues != null
                     && terminatorValues.Contains(token.Value)) {
                        break;
                    }
                }
                token = NextToken();
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
                        throw new Exception($"Internal error: {nameof(mismatchingPairs)} grabbed invalid {nameof(TokenType)}: {mismatch.Type}.");
                    }
                }
                ReportError(errorType, mismatch);
            }
        }

        private Token NextToken() {
            // reset token properties
            tokenStartPosition = stream.Position;
            tokenValue         = "";
#if DEBUG
            if (tokens.Count != 0 && tokens.Last.Value.Type != TokenType.Whitespace) {
                Debug.Assert(tokenStartPosition == (tokens.Last.Value.EndLine, tokens.Last.Value.EndColumn));
            }
#endif
            // newline
            if (c == '\r') {
                stream.Move();
                return null;
            }

            if (c == Spec.EndOfLine) {
                stream.Move();
                // don't add newline as first token
                // and don't create multiple newline tokens.
                if ((tokens.Count == 0 || tokens.Last.Value.Type == TokenType.Newline)
                 && !options.HasFlag(SourceProcessingOptions.PreserveWhitespaces)) {
                    return null;
                }
                return new EndOfLineToken(tokenStartPosition);
            }

            // source end mark
            if (c == Spec.EndOfStream) {
                return new EndOfStreamToken(tokenStartPosition);
            }

            // whitespaces & indentation
            if (c == ' ' || c == '\t') {
                // check if we're at line beginning
                if (tokens.Count != 0 && mismatchingPairs.Count == 0 && tokens.Last.Value.Type == TokenType.Newline) {
                    // set indent character if it is unknown
                    if (indentChar == '\0') {
                        indentChar = c;
                    }

                    // compute indentation length
                    var indentLength = 0;
                    while (c == ' ' || c == '\t') {
                        tokenValue += c;

                        // check for consistency
                        if (!inconsistentIndentation && indentChar != c) {
                            inconsistentIndentation = true;
                        }

                        if (c == ' ') {
                            indentLength++;
                        }
                        else if (c == '\t') {
                            // tab size computation borrowed from Python compiler
                            indentLength += 8 - indentLength % 8;
                        }
                        stream.Move();
                    }

                    // return if line is blank / commented
                    {
                        string restOfLine = stream.GetRestOfLine();
                        if ( // rest of line is blank
                            string.IsNullOrWhiteSpace(restOfLine)
                            // rest is one-line comment
                         || restOfLine.StartsWith(Spec.CommentOneLineStart)
                            // or rest is multiline comment
                         || restOfLine.StartsWith(Spec.CommentMultilineStart)
                            // and comment goes through end of line
                         && Regex.Matches(restOfLine, Spec.CommentMultilineStartPattern).Count
                          > Regex.Matches(restOfLine, Spec.CommentMultilineEndPattern).Count) {
                            return new Token(TokenType.Whitespace, tokenStartPosition, tokenValue);
                        }
                    }

                    Token indentationToken;
                    if (indentLength > indentLevel) {
                        // indent increased
                        indentationToken = new IndentToken(tokenStartPosition, tokenValue);
                    }
                    else if (indentLength < indentLevel) {
                        // indent decreased
                        indentationToken = new OutdentToken(tokenStartPosition, tokenValue);
                    }
                    else {
                        // whitespace
                        indentationToken = new Token(TokenType.Whitespace, tokenStartPosition, tokenValue);
                    }
                    indentLevel = indentLength;

                    // warn user about inconsistency
                    if (inconsistentIndentation && options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                        ReportWarning(
                            WarningType.InconsistentIndentation,
                            indentationToken
                        );
                        // ignore future warnings
                        options &= ~SourceProcessingOptions.CheckIndentationConsistency;
                    }
                    return indentationToken;
                }
                // whitespace
                if (options.HasFlag(SourceProcessingOptions.PreserveWhitespaces)) {
                    char whitespace = c;
                    stream.Move();
                    if (tokens.Count > 0
                     && tokens.Last.Value.Type == TokenType.Whitespace) {
                        tokens.Last.Value.Value += whitespace;
                        return null;
                    }
                    return new Token(TokenType.Whitespace, tokenStartPosition, whitespace.ToString());
                }
                // usefulness whitespace
                stream.Move();
                return null;
            }

            // one-line comment
            if (c.ToString() == Spec.CommentOneLineStart) {
                stream.Move();

                // skip all until end of line or end of file
                while (!stream.AtEndOfLine() && c != Spec.EndOfStream) {
                    tokenValue += c;
                    stream.Move();
                }
                return new OneLineCommentToken(tokenStartPosition, tokenValue);
            }

            // multiline comment
            if (c.ToString() + stream.Peek() == Spec.CommentMultilineStart) {
                stream.Move(2);
                var commentLevel = 1;
                while (commentLevel > 0) {
                    string nextPiece = c.ToString() + stream.Peek();
                    // found comment end
                    if (nextPiece == Spec.CommentMultilineEnd) {
                        // don't add last comment '*/'
                        if (commentLevel != 1) {
                            tokenValue += Spec.CommentMultilineEnd;
                        }
                        stream.Move(2);
                        // decrease comment level
                        commentLevel--;
                    }
                    // found nested multiline comment start
                    else if (nextPiece == Spec.CommentMultilineStart) {
                        tokenValue += Spec.CommentMultilineStart;
                        stream.Move(2);
                        // increase comment level
                        commentLevel++;
                    }
                    // went through end of file
                    else if (c == Spec.EndOfStream) {
                        var token = new MultilineCommentToken(tokenStartPosition, tokenValue, true);
                        ReportError(ErrorType.UnclosedMultilineComment, token);
                        return token;
                    }
                    // found any other character
                    else {
                        tokenValue += c;
                        stream.Move();
                    }
                }
                return new MultilineCommentToken(tokenStartPosition, tokenValue);
            }

            // character literal
            if (c == Spec.CharLiteralQuote) {
                stream.Move();
                // got escape-sequence
                if (c == '\\') {
                    tokenValue += '\\';
                    stream.Move();
                    // got invalid sequence
                    if (!Spec.EscapeSequences.Keys.Contains(c)) {
                        tokenValue += c;
                        var token = new CharacterToken(tokenStartPosition, tokenValue);
                        ReportError(ErrorType.InvalidEscapeSequence, token);
                        return token;
                    }
                }
                else if (c == Spec.CharLiteralQuote) {
                    stream.Move();
                    var token = new CharacterToken(tokenStartPosition, "");
                    ReportError(ErrorType.EmptyCharacterLiteral, token);
                    return token;
                }
                tokenValue += c;
                stream.Move();
                if (c != Spec.CharLiteralQuote) {
                    tokenValue += c;
                    stream.Move();
                    var errorType = ErrorType.CharacterLiteralTooLong;
                    while (c != Spec.CharLiteralQuote) {
                        if (stream.AtEndOfLine() || c == Spec.EndOfStream) {
                            errorType = ErrorType.UnclosedCharacterLiteral;
                            break;
                        }
                        tokenValue += c;
                        stream.Move();
                    }
                    var token = new CharacterToken(tokenStartPosition, tokenValue, true);
                    ReportError(errorType, token);
                    return token;
                }
                stream.Move();
                return new CharacterToken(tokenStartPosition, tokenValue);
            }

            // string
            if (Spec.StringQuotes.Contains(c)) {
                var stringOptions = StringLiteralOptions.None;
                {
                    var tempPosition = tokenStartPosition;
                    int tempCharIdx  = stream.CharIdx;
                    // read string prefixes
                    while (tempPosition.column > 0
                        && Spec.StringPrefixes.TryGetValue(stream.Source[--tempCharIdx], out StringLiteralOptions newOption)) {
                        tempPosition.column--;
                        // check for duplicated prefixes
                        if (stringOptions.HasFlag(newOption)) {
                            ReportWarning(
                                WarningType.DuplicatedStringPrefix,
                                new Token(TokenType.Identifier, tempPosition, c.ToString())
                            );
                            continue;
                        }
                        stringOptions |= newOption;
                    }
                }

                // if has prefixes
                if (stringOptions != StringLiteralOptions.None) {
                    // remove last token - it is prefix meant as identifier.
                    tokenStartPosition.column -= tokens.Last.Value.Value.Length;
                    tokens.RemoveLast();
                }

                string delimiter = c.ToString();
                char   quote     = c;
                stream.Move();
                // add next 2 quotes for multiline strings
                if (c == quote) {
                    delimiter += c;
                    stream.Move();
                    if (c != quote) {
                        // got empty one-quoted string
                        var emptyString = new StringToken(
                            tokenStartPosition,
                            "",
                            quote,
                            stringOptions
                        );
                        if (stringOptions != StringLiteralOptions.None) {
                            ReportWarning(
                                WarningType.RedundantPrefixesForEmptyString,
                                emptyString
                            );
                        }
                        return emptyString;
                    }
                    delimiter += c;
                    stream.Move();
                }
                if (delimiter.Length == 3) {
                    stringOptions |= StringLiteralOptions.Multiline;
                }

                return ReadString(delimiter, stringOptions);
            }

            // number
            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            // identifier/keyword
            if (Spec.IsValidIdStart(c)) {
                tokenValue += c;
                stream.Move();
                while (Spec.IsValidIdChar(c)) {
                    tokenValue += c;
                    stream.Move();
                }
                // remove trailing restricted endings
                int rawIdLength = tokenValue.Length;
                tokenValue = tokenValue.TrimEnd(Spec.RestrictedIdentifierEndings);
                int explicitIdPartLength = rawIdLength - tokenValue.Length;
                if (explicitIdPartLength != 0) {
                    stream.Move(-explicitIdPartLength);
                }

                if (!Spec.Keywords.TryGetValue(tokenValue, out TokenType tokenType)) {
                    return new IdentifierToken(tokenStartPosition, tokenValue);
                }
                return new KeywordToken(tokenStartPosition, tokenValue, tokenType);
            }

            // operator
            if (Spec.OperatorChars.Contains(c)) {
                int    longestLength = Spec.OperatorsValues[0].Length;
                string next          = c + stream.Peek((uint) longestLength - 1u);
                for (int length = longestLength; length > 0; length--) {
                    string piece = next.Substring(0, length);
                    if (Spec.OperatorsValues.Contains(piece)) {
                        stream.Move(length);
                        var operatorToken = new OperatorToken(tokenStartPosition, piece);
                        // got open brace
                        if (operatorToken.Properties.IsOpenBrace) {
                            mismatchingPairs.Add(operatorToken);
                        }
                        else if (operatorToken.Properties.IsCloseBrace) {
                            // got mismatching close brace
                            if (mismatchingPairs.Count == 0) {
                                mismatchingPairs.Add(operatorToken);
                            }
                            // got matching close brace
                            else if (mismatchingPairs.Last().Type == operatorToken.Properties.MatchingBrace) {
                                mismatchingPairs.RemoveAt(mismatchingPairs.Count - 1);
                            }
                        }
                        return operatorToken;
                    }
                }
                var invalidOperatorToken = new OperatorToken(tokenStartPosition, Spec.InvalidOperatorProperties);
                ReportError(ErrorType.InvalidOperator, invalidOperatorToken);
                return invalidOperatorToken;
            }

            // invalid
            tokenValue += c;
            stream.Move();
            var unknownToken = new Token(TokenType.Unknown, tokenStartPosition, tokenValue);
            ReportError(ErrorType.InvalidSymbol, unknownToken);
            return unknownToken;
        }

        #region Parsing Number literals

        private Token ReadNumber() {
            if (c == '0') {
                tokenValue += "0";
                stream.Move();

                int           radix;
                NumberOptions numberOptions;
                numberOptions.Bits      = 32;
                numberOptions.Floating  = false;
                numberOptions.Imaginary = false;
                numberOptions.Unsigned  = false;
                numberOptions.Unlimited = false;
                bool      atFirstChar;
                ErrorType errorType;

                // second char (determines radix)
                if (c == 'b' || c == 'B') {
                    tokenValue += c;
                    stream.Move();
                    radix = 2;
                    ReadBinaryNumber(ref numberOptions, out atFirstChar, out errorType);
                }
                else if (c == 'o' || c == 'O') {
                    tokenValue += c;
                    stream.Move();
                    radix = 8;
                    ReadOctalNumber(ref numberOptions, out atFirstChar, out errorType);
                }
                else if (c == 'x' || c == 'X') {
                    tokenValue += c;
                    stream.Move();
                    radix = 16;
                    ReadHexNumber(ref numberOptions, out atFirstChar, out errorType);
                }
                else {
                    // skip leading zeros
                    while (c == '0') {
                        tokenValue += "0";
                        stream.Move();
                    }
                    return ReadDecimalNumber(true);
                }
                if (atFirstChar) {
                    errorType = ErrorType.ExpectedDigitAfterNumberBaseSpecifier;
                }
                if (errorType != ErrorType.None) {
                    var invalidNumber = new NumberToken(tokenStartPosition, tokenValue, numberOptions);
                    ReportError(
                        errorType,
                        invalidNumber
                    );
                    return invalidNumber;
                }
                if (numberOptions.Unlimited) {
                    BigInteger value = LiteralParser.ParseBigInteger(tokenValue.Substring(2, tokenValue.Length - 3), radix);
                }
                else {
                    object value = LiteralParser.ParseInteger(tokenValue.Substring(2), radix);
                }
                return new NumberToken(tokenStartPosition, tokenValue, numberOptions);
            }
            return ReadDecimalNumber(false);
        }

        private Token ReadDecimalNumber(bool startsWithZero) {
            // c is something except 0 here
            NumberOptions numberOptions;
            numberOptions.Bits      = 32;
            numberOptions.Floating  = false;
            numberOptions.Imaginary = false;
            numberOptions.Unlimited = false;
            numberOptions.Unsigned  = false;
            var atFirstChar = true;
            var errorType   = ErrorType.None;
            // TODO: maybe use dynamic as value of number?
            while (Spec.IsValidNumberPart(c)) {
                if (numberOptions.Unlimited) {
                    errorType = ErrorType.ExpectedEndOfNumberAfterLongPostfix;
                }
                if (!char.IsDigit(c) && c != '_') {
                    if (c == '.') { // BUG: no error after dot
                        if (char.IsDigit(stream.Peek())) {
                            // if found second dot
                            if (numberOptions.Floating) {
                                tokenValue += c;
                                stream.Move();
                                var invalidNumber = new NumberToken(tokenStartPosition, tokenValue, numberOptions);
                                ReportError(ErrorType.RepeatedDotInNumberLiteral, invalidNumber);
                                return invalidNumber;
                            }
                            numberOptions.Floating = true;
                            numberOptions.Bits = 64;
                        }
                        // non-digit after dot: '.' is operator on some number
                        // leave dot to next token.
                        else {
                            break;
                        }
                    }
                    else if (c == 'j' || c == 'J') {
                        if (numberOptions.Imaginary) {
                            errorType = ErrorType.RepeatedImaginarySignInNumberLiteral;
                        }
                        numberOptions.Imaginary = true;
                    }
                    else if (c == 'e' || c == 'E') {
                        ReadExponent(numberOptions, ref errorType);
                        continue;
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            ref numberOptions,
                            false,
                            ref errorType
                        );
                        continue;
                    }
                    else {
                        // invalid
                        if (startsWithZero && !atFirstChar) {
                            tokenValue += c;
                            var errorToken = new Token(
                                TokenType.Unknown,
                                tokenStartPosition,
                                tokenValue
                            );
                            ReportError(ErrorType.InvalidIntegerLiteral, errorToken);
                            return errorToken;
                        }
                        break;
                    }
                }
                tokenValue += c;
                stream.Move();
                atFirstChar = false;
            }
            var number = new NumberToken(tokenStartPosition, tokenValue, numberOptions);
            if (errorType != ErrorType.None) {
                ReportError(errorType, number);
            }
            return number;
        }

        private void ReadBinaryNumber(
            ref NumberOptions numberOptions,
            out bool          atFirstChar,
            out ErrorType     errorType
        ) {
            var        bitsCount = 0;
            var        longValue = 0L;
            BigInteger bigInt    = BigInteger.Zero;
            atFirstChar = true;
            errorType   = ErrorType.None;
            while (Spec.IsValidNumberPart(c)) {
                if (numberOptions.Unlimited) {
                    errorType = ErrorType.ExpectedEndOfNumberAfterLongPostfix;
                }
                switch (c) {
                    case '0':
                        if (longValue != 0L) {
                            // ignore leading 0's...
                            goto case '1';
                        }
                        break;
                    case '1':
                        bitsCount++;
                        if (bitsCount > 8) {
                            numberOptions.Bits = 16;
                        }
                        else if (bitsCount > 16) {
                            numberOptions.Bits = 32;
                        }
                        else if (bitsCount > 32) {
                            numberOptions.Bits = 64;
                        }
                        else if (bitsCount > 64) {
                            numberOptions.Bits = 128;
                            bigInt             = longValue;
                        }
                        // TODO debug
                        var or = (byte) (c - '0');
                        if (bitsCount >= 64) {
                            bigInt = (bigInt << 1) | or;
                        }
                        else {
                            longValue = (longValue << 1) | or;
                        }
                        break;
                    case '_': {
                        break;
                    }
                    default: {
                        if (Spec.NumberPostfixes.Contains(c)) {
                            ReadNumberPostfix(
                                ref numberOptions,
                                false,
                                ref errorType
                            );
                            continue;
                        }
                        // invalid
                        errorType = ErrorType.InvalidBinaryLiteral;
                        break;
                    }
                }
                tokenValue += c;
                stream.Move();
                atFirstChar = false;
            }
        }

        private void ReadOctalNumber(
            ref NumberOptions numberOptions,
            out bool          atFirstChar,
            out ErrorType     errorType
        ) {
            numberOptions.Bits      = 32;
            numberOptions.Unsigned  = false;
            numberOptions.Unlimited = false;
            atFirstChar             = true;
            errorType               = ErrorType.None;
            while (Spec.IsValidNumberPart(c)) {
                if (numberOptions.Unlimited) {
                    errorType = ErrorType.ExpectedEndOfNumberAfterLongPostfix;
                }
                if (!c.IsValidOctalDigit()) {
                    if (c == '_') {
                        tokenValue += c;
                        stream.Move();
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            ref numberOptions,
                            atFirstChar,
                            ref errorType
                        );
                        continue;
                    }
                    else {
                        // invalid
                        errorType = ErrorType.InvalidOctalLiteral;
                    }
                }
                tokenValue += c;
                stream.Move();
                atFirstChar = false;
            }
        }

        private void ReadHexNumber(
            ref NumberOptions numberOptions,
            out bool          atFirstChar,
            out ErrorType     errorType
        ) {
            numberOptions.Bits      = 32;
            numberOptions.Unsigned  = false;
            numberOptions.Unlimited = false;
            atFirstChar             = true;
            errorType               = ErrorType.None;
            while (Spec.IsValidNumberPart(c)) {
                if (numberOptions.Unlimited) {
                    errorType = ErrorType.ExpectedEndOfNumberAfterLongPostfix;
                }
                if (!c.IsValidHexadecimalDigit()) {
                    if (c == '_') {
                        tokenValue += c;
                        stream.Move();
                    }
                    else if (Spec.NumberPostfixes.Contains(c)) {
                        ReadNumberPostfix(
                            ref numberOptions,
                            atFirstChar,
                            ref errorType
                        );
                        continue;
                    }
                    else {
                        // invalid
                        errorType = ErrorType.InvalidHexadecimalLiteral;
                    }
                }
                tokenValue += c;
                stream.Move();
                atFirstChar = false;
            }
        }

        private void ReadExponent(
            NumberOptions numberOptions,
            ref ErrorType errorType
        ) {
            // c == 'e'
            var hasExponentValue = false;
            tokenValue += c;
            stream.Move();
            if (c == '-' || c == '+') {
                tokenValue += c;
                stream.Move();
            }
            //object value;
            while (Spec.IsValidNumberPart(c)) {
                if (char.IsDigit(c)) {
                    hasExponentValue = true;
                }
                else if (c == 'j' || c == 'J') {
                    if (numberOptions.Imaginary) {
                        errorType = ErrorType.RepeatedImaginarySignInNumberLiteral;
                    }
                    numberOptions.Imaginary = true;
                }
                else if (Spec.NumberPostfixes.Contains(c)) {
                    ReadNumberPostfix(
                        ref numberOptions,
                        false,
                        ref errorType
                    );
                    continue;
                }
                else {
                    // invalid
                    errorType = ErrorType.InvalidValueAfterExponent;
                    break;
                }
                tokenValue += c;
                stream.Move();
            }
            if (!hasExponentValue) {
                errorType = ErrorType.ExpectedDigitAfterNumberExponent;
            }
        }

        private void ReadNumberPostfix(
            ref NumberOptions numberOptions,
            bool              atFirstChar,
            ref ErrorType     errorType
        ) {
            var bitRateStr = "";
            if (atFirstChar) {
                errorType = ErrorType.ExpectedDigitAfterNumberBaseSpecifier;
            }
            else if (numberOptions.Unlimited) {
                errorType = ErrorType.ShouldHaveNoValueAfterNumberLongPostfix;
            }
            // read postfix
            if (c == 'i' || c == 'I') {
                numberOptions.Unsigned = false;
            }
            else if (c == 'u' || c == 'U') {
                numberOptions.Unsigned = true;
            }
            else if (c == 'f' || c == 'F') {
                numberOptions.Floating = true;
            }
            else if (c == 'l' || c == 'L') {
                numberOptions.Unlimited = true;
                return;
            }
            else {
                errorType = ErrorType.InvalidPostfixInNumberLiteral;
                return;
            }

            // postfix is 'i' or 'u' here
            tokenValue += c;
            stream.Move();
            if (char.IsDigit(c)) {
                while (char.IsDigit(c)) {
                    tokenValue += c;
                    bitRateStr += c;
                    stream.Move();
                }
                if (!int.TryParse(bitRateStr, out numberOptions.Bits)
                 || !Spec.NumberFloatBitRates.Contains(numberOptions.Bits)) {
                    errorType = ErrorType.InvalidIntegerNumberBitRate;
                    numberOptions.Bits = 32;
                }
                if (numberOptions.Floating
                 && !Spec.NumberFloatBitRates.Contains(numberOptions.Bits)) {
                    errorType          = ErrorType.InvalidFloatNumberBitRate;
                    numberOptions.Bits = 64;
                }
            }
            else {
                // expected digit after num 'i#' postfix
                errorType = ErrorType.ExpectedABitRateAfterNumberPostfix;
            }
        }

        #endregion

        private Token ReadString(string delimiter, StringLiteralOptions stringOptions) {
            char quote = delimiter[0];

            bool isRaw                = stringOptions.HasFlag(StringLiteralOptions.Raw);
            bool isFmt                = stringOptions.HasFlag(StringLiteralOptions.Format);
            bool normalizeLineEndings = stringOptions.HasFlag(StringLiteralOptions.NormalizeLineEndings);

            var interpolations = new List<Interpolation>();
            int startIndex     = stream.CharIdx;

            while (true) {
                if (c == '\\') {
                    stream.Move();
                    // just '\' and char
                    if (isRaw) {
                        tokenValue += "\\" + c;
                        continue;
                    }
                    // escape sequence
                    if (Spec.EscapeSequences.TryGetValue(c, out string value)) {
                        tokenValue += value;
                        continue;
                    }
                    switch (c) {
                        // unicode symbol
                        case 'u':
                        case 'U': {
                            int unicodeSymLen = c == 'u' ? 4 : 8;
                            if (LiteralParser.TryParseInt(stream.Source, stream.CharIdx, unicodeSymLen, 16, out int val)) {
                                if (val < 0 || val > 0x10ffff) {
                                    throw new Exception(
                                        $"Can't decode bytes at line {stream.Position.line}, column {stream.Position.column}: illegal Unicode character"
                                    );
                                }
                                if (val < 0x010000) {
                                    tokenValue += (char) val;
                                }
                                else {
                                    tokenValue += char.ConvertFromUtf32(val);
                                }
                                stream.Move(unicodeSymLen);
                            }
                            else {
                                throw new Exception(
                                    $"Can't decode bytes at line {stream.Position.line}, column {stream.Position.column}: truncated \\uXXXX escape"
                                );
                            }
                            continue;
                        }
                        // newlines
                        case '\r': {
                            if (c == '\n') {
                                stream.Move();
                            }
                            continue;
                        }
                        case '\n': {
                            continue;
                        }
                        // hexadecimal char
                        case 'x': {
                            if (!LiteralParser.TryParseInt(stream.Source, stream.CharIdx, 2, 16, out int val)) {
                                goto default;
                            }
                            tokenValue += (char) val;
                            stream.Move(2);
                            continue;
                        }
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7': {
                            int val = c - '0';
                            if (LiteralParser.HexValue(c, out int oneChar) && oneChar < 8) {
                                val = val * 8 + oneChar;
                                stream.Move();
                                if (LiteralParser.HexValue(c, out oneChar) && oneChar < 8) {
                                    val = val * 8 + oneChar;
                                    stream.Move();
                                }
                            }
                            tokenValue += (char) val;
                            continue;
                        }
                        default: {
                            tokenValue += "\\" + c;
                            continue;
                        }
                    }
                }

                // found end of line
                if (c == '\r' && normalizeLineEndings) {
                    // normalize line endings
                    if (c == '\n') {
                        stream.Move();
                    }
                    tokenValue += '\n';
                }

                // check for end of line/file
                if (stream.AtEndOfLine() && delimiter.Length == 1
                 || c == Spec.EndOfStream) {
                    var token = new StringToken(
                        tokenStartPosition,
                        tokenValue,
                        quote,
                        stringOptions,
                        true
                    );
                    ReportError(ErrorType.UnclosedString, token);
                    return token;
                }

                // got non-escaped quote
                if (c == quote && (tokenValue.Length == 0 || tokenValue[tokenValue.Length - 1] != '\\')) {
                    stream.Move();
                    // ending "
                    if (delimiter.Length == 1) {
                        break;
                    }
                    // ending """
                    if (c == quote && stream.Peek() == quote) {
                        stream.Move(2);
                        break;
                    }
                    // just piece of string
                    stream.Move();
                }

                // found string format sign
                if (c == '{' && isFmt) {
                    ReadStringInterpolation(interpolations, startIndex);
                    continue;
                }
                // else
                tokenValue += c;
                stream.Move();
            }
            if (isFmt) {
                var interpolatedStringToken = new InterpolatedStringToken(tokenStartPosition, tokenValue, quote, stringOptions, interpolations);
                if (interpolations.Count == 0) {
                    ReportWarning(
                        WarningType.RedundantStringFormatPrefix,
                        interpolatedStringToken
                    );
                }
                return interpolatedStringToken;
            }
            return new StringToken(tokenStartPosition, tokenValue, quote, stringOptions);
        }

        private void ReadStringInterpolation(ICollection<Interpolation> interpolations, int stringStartIndex) {
            var newInterpolation = new Interpolation(stream.CharIdx - stringStartIndex);
            interpolations.Add(newInterpolation);
            // process interpolation
            {
                var lexer = new Lexer(
                    stream,
                    newInterpolation.Tokens,
                    errors,
                    warnings,
                    options,
                    new[] { "}" }
                );
                lexer.stream.Move(); // skip {
                lexer.mismatchingPairs.Add(new Token(TokenType.OpLeftBrace, stream.Position, "{"));
                lexer.Process();
                // remove usefulness closing curly
                newInterpolation.Tokens.RemoveLast();
                // restore character position
                stream = new CharStream(lexer.stream);
            }
            // append interpolated piece to main string token
            newInterpolation.EndIndex =  stream.CharIdx - stringStartIndex;
            tokenValue                += stream.Source.Substring(stream.CharIdx - newInterpolation.Length, newInterpolation.Length);
        }

        private void ReportError(ErrorType occurredErrorType, Token token) {
            Debug.Assert(occurredErrorType != ErrorType.None);

            errors.Add(new SyntaxError(occurredErrorType, stream.Source, token));
        }

        private void ReportWarning(WarningType occurredWarningType, Token token) {
            Debug.Assert(occurredWarningType != WarningType.None);

            warnings.Add(new SyntaxError(occurredWarningType, stream.Source, token));
        }
    }
}