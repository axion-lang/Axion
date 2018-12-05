using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Axion.Core.Tokens;

namespace Axion.Core.Processing {
    /// <summary>
    ///     Static tool for splitting Axion code into tokens <see cref="LinkedList{T}" />.
    /// </summary>
    public partial class Lexer : AbstractLexer {
        #region Current source properties

        private char c => Stream.C;

        /// <summary>
        ///     Contains values, from which lexer should stop working.
        /// </summary>
        private string[] _processingTerminators;

        /// <summary>
        ///     Contains all unclosed multiline comments found.
        /// </summary>
        private List<MultilineCommentToken> _unclosedMultilineComments { get; set; }

        /// <summary>
        ///     Contains all unclosed strings found in source code.
        /// </summary>
        private List<StringToken> _unclosedStrings { get; set; }

        /// <summary>
        ///     Contains unpaired parenthesis, brackets and braces.
        /// </summary>
        private List<Token> _mismatchingPairs { get; } = new List<Token>();

        #endregion

        #region Current token properties

        /// <summary>
        ///     Value of current reading <see cref="Token" />
        /// </summary>
        private readonly StringBuilder tokenValue = new StringBuilder();

        /// <summary>
        ///     Start position of current reading <see cref="Token" />.
        /// </summary>
        private (int line, int column) tokenStartPosition = (0, 0);

        #endregion

        #region Indentation properties

        /// <summary>
        ///     .
        /// </summary>
        private int indentLevel;

        /// <summary>
        ///     Last code line indentation length.
        /// </summary>
        private int lastIndentLength;

        /// <summary>
        ///     Size of one indentation in source.
        ///     Determined by first indentation.
        /// </summary>
        private int oneIndentSize;

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

        public Lexer(
            string                  codeToProcess,
            LinkedList<Token>       outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) : base(codeToProcess, outTokens, outErrors, outWarnings, processingOptions) {
            AddPresets();
        }

        public Lexer(
            CharStream              fromStream,
            LinkedList<Token>       outTokens,
            List<Exception>         outErrors,
            List<Exception>         outWarnings,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) : base(fromStream, outTokens, outErrors, outWarnings, processingOptions) {
            AddPresets();
        }

        protected sealed override void AddPresets(
            List<MultilineCommentToken> unclosedMultilineComments = null,
            List<StringToken>           unclosedStrings           = null,
            string[]                    processingTerminators     = null
        ) {
            _unclosedStrings           = unclosedStrings ?? new List<StringToken>();
            _unclosedMultilineComments = unclosedMultilineComments ?? new List<MultilineCommentToken>();
            _processingTerminators     = processingTerminators ?? new string[0];
        }

        /// <summary>
        ///     Divides code into list of tokens.
        /// </summary>
        public override void Process() {
            Token token = null;
            // at first, check if we're after unclosed string
            if (_unclosedStrings.Count > 0) {
                token = ReadString(true);
            }
            // then, check if we're after unclosed multiline comment
            if (_unclosedMultilineComments.Count > 0) {
                token = ReadMultilineComment();
            }
            if (token == null) {
                token = NextToken();
            }
            while (true) {
                if (token != null) {
                    Tokens.AddLast(token);
                    // check for processing terminator
                    if (token.Type == TokenType.EndOfStream
                     || _processingTerminators.Contains(token.Value)) {
                        break;
                    }
                }
                token = NextToken();
            }

            #region Process mismatches

            for (var i = 0; i < _mismatchingPairs.Count; i++) {
                Token     mismatch = _mismatchingPairs[i];
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
                        throw new NotSupportedException(
                            "Internal error: " + nameof(_mismatchingPairs) +
                            " grabbed invalid " + nameof(TokenType) + ": " + mismatch.Type
                        );
                    }
                }
                ReportError(errorType, mismatch);
            }

            #endregion
        }

        /// <summary>
        ///     Read next token from character stream.
        /// </summary>
        private Token NextToken() {
            // reset token properties
            tokenStartPosition = Stream.Position;
            tokenValue.Clear();
//#if DEBUG
//            if (Tokens.Count != 0 && Tokens.Last.Value.Type != TokenType.Outdent) {
//                Debug.Assert(tokenStartPosition == (Tokens.Last.Value.EndLine, Tokens.Last.Value.EndColumn));
//            }
//#endif
            // source end mark
            if (c == Spec.EndOfStream) {
                return new EndOfStreamToken(tokenStartPosition);
            }

            // newline
            if (c == '\r') {
                Stream.Move();
                if (c == '\n') {
                    tokenValue.Append('\r');
                }
                else {
                    return null;
                }
            }

            if (c == '\n') {
                return ReadNewline();
            }

            // whitespaces & indentation
            if (Spec.IsSpaceOrTab(c)) {
                if (Tokens.Count != 0
                 && _mismatchingPairs.Count == 0
                 && Tokens.Last.Value.Type == TokenType.Newline) {
                    return ReadIndentation();
                }
                return ReadWhitespace();
            }

            // one-line comment
            if (c.ToString() == Spec.SingleCommentStart) {
                return ReadSingleComment();
            }

            // multiline comment
            if (c.ToString() + Stream.Peek() == Spec.MultiCommentStart) {
                return ReadMultilineComment();
            }

            // character literal
            if (c == Spec.CharLiteralQuote) {
                return ReadCharLiteral();
            }

            // string
            if (Spec.StringQuotes.Contains(c)) {
                return ReadString(false);
            }

            // number
            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            // identifier/keyword
            if (Spec.IsValidIdStart(c)) {
                return ReadIdentifier();
            }

            // operator
            if (Spec.OperatorChars.Contains(c)) {
                return ReadOperator();
            }

            // invalid
            tokenValue.Append(c);
            Stream.Move();
            var unknownToken = new Token(TokenType.Unknown, tokenStartPosition, tokenValue.ToString());
            ReportError(ErrorType.InvalidSymbol, unknownToken);
            return unknownToken;
        }

        #region Reading tokens

        private EndOfLineToken ReadNewline() {
            // skip all newline characters
            while (c == '\n' || c == '\r') {
                tokenValue.Append(c);
                Stream.Move();
            }
            var endOfLineToken = new EndOfLineToken(tokenStartPosition, tokenValue.ToString());
            if (Spec.IsSpaceOrTab(c)) {
                return endOfLineToken;
            }

            // if last newline doesn't starts
            // with whitespace - reset indentation to 0
            // add newline at first
            Tokens.AddLast(endOfLineToken);
            tokenStartPosition = Stream.Position;
            // then add outdents
            lastIndentLength = 0;
            while (indentLevel > 0) {
                Tokens.AddLast(new OutdentToken(tokenStartPosition));
                indentLevel--;
            }
            return null;
        }

        private Token ReadWhitespace() {
            var whitespace = "";
            while (c == ' ' || c == '\t') {
                whitespace += c;
                Stream.Move();
            }
            if (Tokens.Count > 0) {
                Tokens.Last.Value.AppendWhitespace(whitespace);
                return null;
            }
            // if it is 1st token,
            // set default indentation level.
            // TODO use another approach with indentation (leadingWhitespaces property)
            lastIndentLength = whitespace.Length;
            return new Token(TokenType.Whitespace, tokenStartPosition, "", whitespace);
        }

        private SingleCommentToken ReadSingleComment() {
            Stream.Move();

            // skip all until end of line or end of file
            while (!Stream.AtEndOfLine() && c != Spec.EndOfStream) {
                tokenValue.Append(c);
                Stream.Move();
            }
            return new SingleCommentToken(tokenStartPosition, tokenValue.ToString());
        }

        private Token ReadIndentation() {
            // set indent character if it is unknown
            if (indentChar == '\0') {
                indentChar = c;
            }

            // compute indentation length
            var newIndentLength = 0;
            while (Spec.IsSpaceOrTab(c)) {
                tokenValue.Append(c);

                // check for consistency
                if (!inconsistentIndentation && indentChar != c) {
                    inconsistentIndentation = true;
                }

                if (c == ' ') {
                    newIndentLength++;
                }
                else if (c == '\t') {
                    // tab size computation borrowed from Python compiler
                    newIndentLength += 8 - newIndentLength % 8;
                }
                Stream.Move();
            }

            if (oneIndentSize == 0 && !inconsistentIndentation) {
                oneIndentSize = newIndentLength;
            }

            // return if line is blank / commented
            {
                string restOfLine = Stream.GetRestOfLine();
                if ( // rest of line is blank
                    string.IsNullOrWhiteSpace(restOfLine)
                    // rest is one-line comment
                 || restOfLine.StartsWith(Spec.SingleCommentStart)
                    // or rest is multiline comment
                 || restOfLine.StartsWith(Spec.MultiCommentStart)
                    // and comment goes through end of line
                 && Regex.Matches(restOfLine, Spec.MultiCommentStartPattern).Count
                  > Regex.Matches(restOfLine, Spec.MultiCommentEndPattern).Count) {
                    // append it to last token
                    Tokens.Last.Value.AppendWhitespace(tokenValue.ToString());
                    return null;
                }
            }

            Token indentationToken;
            if (newIndentLength > lastIndentLength) {
                // indent increased
                indentationToken = new IndentToken(tokenStartPosition, tokenValue.ToString());
                indentLevel++;
            }
            else if (newIndentLength < lastIndentLength) {
                // whitespace
                if (Tokens.Count > 0) {
                    // append it to last token
                    Tokens.Last.Value.AppendWhitespace(tokenValue.ToString());
                }

                int temp = newIndentLength;
                while (temp < lastIndentLength) {
                    // indent decreased
                    Tokens.AddLast(new OutdentToken(tokenStartPosition));
                    indentLevel--;
                    temp += oneIndentSize;
                }
                indentationToken = null;
            }
            else {
                // whitespace
                if (Tokens.Count > 0) {
                    // append it to last token
                    Tokens.Last.Value.AppendWhitespace(tokenValue.ToString());
                }
                return null;
            }
            lastIndentLength = newIndentLength;

            // warn user about inconsistency
            if (inconsistentIndentation && Options.HasFlag(SourceProcessingOptions.CheckIndentationConsistency)) {
                ReportWarning(
                    WarningType.InconsistentIndentation,
                    indentationToken
                );
                // ignore future warnings
                Options &= ~SourceProcessingOptions.CheckIndentationConsistency;
            }
            return indentationToken;
        }

        /// <summary>
        ///     Gets a multiline comment from next piece of source.
        /// </summary>
        /// <returns>
        ///     Valid multiline comment token if next piece of source
        ///     if comment value is followed by multiline
        ///     comment closing sign.
        ///     Otherwise, returns unclosed multiline comment token.
        /// </returns>
        private MultilineCommentToken ReadMultilineComment() {
            if (_unclosedMultilineComments.Count == 0) {
                // we're on '/*'
                Stream.Move(Spec.MultiCommentStart.Length);
                _unclosedMultilineComments.Add(
                    new MultilineCommentToken(
                        tokenStartPosition,
                        "",
                        true
                    )
                );
            }
            while (_unclosedMultilineComments.Count > 0) {
                string nextPiece = c.ToString() + Stream.Peek();
                // found comment end
                if (nextPiece == Spec.MultiCommentEnd) {
                    // don't add last comment '*/'
                    if (_unclosedMultilineComments.Count != 1) {
                        tokenValue.Append(Spec.MultiCommentEnd);
                    }
                    Stream.Move(2);
                    // decrease comment level
                    _unclosedMultilineComments.RemoveAt(_unclosedMultilineComments.Count - 1);
                }
                // found nested multiline comment start
                else if (nextPiece == Spec.MultiCommentStart) {
                    tokenValue.Append(Spec.MultiCommentStart);
                    Stream.Move(2);
                    // increase comment level
                    _unclosedMultilineComments.Add(
                        new MultilineCommentToken(
                            tokenStartPosition,
                            tokenValue.ToString(),
                            true
                        )
                    );
                }
                // went through end of file
                else if (c == Spec.EndOfStream) {
                    var token = new MultilineCommentToken(
                        tokenStartPosition,
                        tokenValue.ToString(),
                        true
                    );
                    ReportError(ErrorType.UnclosedMultilineComment, token);
                    return token;
                }
                // found any other character
                else {
                    tokenValue.Append(c);
                    Stream.Move();
                }
            }
            return new MultilineCommentToken(tokenStartPosition, tokenValue.ToString());
        }

        /// <summary>
        ///     Gets a character literal from next piece of source.
        /// </summary>
        /// <returns>
        ///     Valid char literal if next piece of source
        ///     is either validly escape sequence or just a char
        ///     with length of 1.
        ///     Otherwise, returns error token,
        ///     that can be either too long char literal,
        ///     or just unclosed char literal.
        /// </returns>
        private CharacterToken ReadCharLiteral() {
            string unescapedValue = null;
            // we're on char quote
            Stream.Move();
            switch (c) {
                case '\\': {
                    // escape-sequence
                    Stream.Move();
                    if (Spec.EscapeSequences.TryGetValue(c, out string sequence)) {
                        tokenValue.Append(sequence);
                        unescapedValue += "\\" + c;
                        Stream.Move();
                    }
                    else {
                        // invalid escape sequence
                        var invalidlyEscapedCharToken = new CharacterToken(tokenStartPosition, "\\" + c);
                        ReportError(ErrorType.InvalidEscapeSequence, invalidlyEscapedCharToken);
                        Stream.Move();
                        return invalidlyEscapedCharToken;
                    }
                    break;
                }
                case Spec.CharLiteralQuote: {
                    Stream.Move();
                    var emptyCharToken = new CharacterToken(tokenStartPosition, "");
                    ReportError(ErrorType.EmptyCharacterLiteral, emptyCharToken);
                    return emptyCharToken;
                }
                default: {
                    // any character
                    tokenValue.Append(c);
                    Stream.Move();
                    break;
                }
            }

            if (c == Spec.CharLiteralQuote) {
                // OK, valid literal
                Stream.Move();
                return new CharacterToken(tokenStartPosition, tokenValue.ToString(), unescapedValue);
            }

            var errorType = ErrorType.CharacterLiteralTooLong;

            tokenValue.Append(c);
            unescapedValue += c;
            Stream.Move();
            while (c != Spec.CharLiteralQuote) {
                if (Stream.AtEndOfLine() || c == Spec.EndOfStream) {
                    errorType = ErrorType.UnclosedCharacterLiteral;
                    break;
                }
                tokenValue.Append(c);
                unescapedValue += c;
                Stream.Move();
            }

            // can be or too long, or unclosed.
            var invalidCharToken = new CharacterToken(tokenStartPosition, tokenValue.ToString(), unescapedValue, true);
            ReportError(errorType, invalidCharToken);
            return invalidCharToken;
        }

        /// <summary>
        ///     Gets a language keyword or identifier from next piece of source.
        /// </summary>
        /// <returns>
        ///     Keyword token if next piece of source
        ///     is keyword, declared in specification.
        ///     Otherwise, returns identifier token.
        /// </returns>
        private Token ReadIdentifier() {
            // don't use StringBuilder, IDs
            // are mostly < 50 characters length.
            string id = "" + c;
            Stream.Move();
            while (Spec.IsValidIdChar(c)) {
                id += c;
                Stream.Move();
            }

            // remove trailing restricted endings
            tokenValue.Append(id.TrimEnd(Spec.RestrictedIdentifierEndings));
            int explicitIdPartLength = id.Length - tokenValue.Length;
            if (explicitIdPartLength != 0) {
                Stream.Move(-explicitIdPartLength);
            }

            if (Spec.Keywords.TryGetValue(tokenValue.ToString(), out TokenType kwType)) {
                return new KeywordToken(kwType, tokenStartPosition);
            }
            return new IdentifierToken(tokenStartPosition, tokenValue.ToString());
        }

        /// <summary>
        ///     Gets a language operator from next piece of source.
        /// </summary>
        /// <returns>
        ///     Operator token if next piece of source
        ///     contains operator, declared in specification.
        ///     Otherwise, returns invalid operator token.
        /// </returns>
        private OperatorToken ReadOperator() {
            int    longestLength = Spec.OperatorsValues[0].Length;
            string nextCodePiece = c + Stream.Peek(longestLength - 1);

            for (int length = nextCodePiece.Length; length > 0; length--) {
                string piece = nextCodePiece.Substring(0, length);
                if (!Spec.OperatorsValues.Contains(piece)) {
                    continue;
                }
                var operatorToken = new OperatorToken(tokenStartPosition, piece);
                if (operatorToken.Properties.IsOpenBrace) {
                    _mismatchingPairs.Add(operatorToken);
                }
                else if (operatorToken.Properties.IsCloseBrace) {
                    if (_mismatchingPairs.Count == 0) {
                        // got mismatching close brace (closing without opening)
                        _mismatchingPairs.Add(operatorToken);
                    }
                    else if (_mismatchingPairs.Last().Type == operatorToken.Properties.GetMatchingBrace()) {
                        // got matching close brace (closing & opening)
                        _mismatchingPairs.RemoveAt(_mismatchingPairs.Count - 1);
                    }
                }
                Stream.Move(length);
                return operatorToken;
            }
            // operator not found in specification
            var invalidOperatorToken = new OperatorToken(tokenStartPosition, Spec.InvalidOperatorProperties);
            ReportError(ErrorType.InvalidOperator, invalidOperatorToken);
            return invalidOperatorToken;
        }

        #endregion
    }
}