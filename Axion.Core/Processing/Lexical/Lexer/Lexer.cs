using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Lexer {
    /// <summary>
    ///     Tool for splitting Axion
    ///     code into list of tokens.
    /// </summary>
    public partial class Lexer {
        #region Current source properties

        /// <summary>
        ///     Current processing <see cref="SourceUnit"/>.
        /// </summary>
        private readonly SourceUnit unit;

        /// <summary>
        ///     Resulting tokens list.
        /// </summary>
        private readonly List<Token> tokens;

        /// <summary>
        ///     Current processing stream.
        /// </summary>
        private CharStream stream;

        /// <summary>
        ///     Short reference to current char in stream.
        /// </summary>
        private char c => stream.C;

        /// <summary>
        ///     Contains values, from which lexer should stop working.
        /// </summary>
        private string[] processingTerminators;

        /// <summary>
        ///     Contains all unclosed multiline comments found.
        /// </summary>
        private List<MultilineCommentToken> unclosedMultilineComments { get; set; }

        /// <summary>
        ///     Contains all unclosed strings found in source code.
        /// </summary>
        private List<StringToken> unclosedStrings { get; set; }

        /// <summary>
        ///     Contains unpaired parenthesis, brackets and braces.
        /// </summary>
        private List<Token> mismatchingPairs { get; } = new List<Token>();

        #endregion

        #region Current token properties

        /// <summary>
        ///     Value of current reading <see cref="Token" />
        /// </summary>
        private readonly StringBuilder tokenValue = new StringBuilder();

        /// <summary>
        ///     Start position of current reading <see cref="Token" />.
        /// </summary>
        private Position tokenStartPosition = (0, 0);

        #endregion

        #region Constructors

        public Lexer(SourceUnit unit) {
            this.unit = unit;
            stream    = new CharStream(unit.Code);
            tokens    = unit.Tokens;
            AddPresets();
        }

        private Lexer(
            SourceUnit  unit,
            CharStream  fromStream,
            List<Token> outTokens
        ) {
            this.unit = unit;
            stream    = new CharStream(fromStream);
            tokens    = outTokens ?? new List<Token>();
            AddPresets();
        }

        #endregion

        /// <summary>
        ///     Divides code into list of tokens.
        /// </summary>
        public void Process() {
            if (string.IsNullOrWhiteSpace(stream.Source)) {
                return;
            }

            Token token = null;
            // at first, check if we're after unclosed string
            if (unclosedStrings.Count > 0) {
                token = ReadString(true);
            }

            // then, check if we're after unclosed multiline comment
            if (unclosedMultilineComments.Count > 0) {
                token = ReadMultilineComment();
            }

            if (token == null) {
                token = NextToken();
            }

            while (true) {
                if (token != null) {
                    tokens.Add(token);
                    // check for processing terminator
                    if (token.Is(EndOfCode)
                        || processingTerminators.Contains(token.Value)) {
                        break;
                    }
                }

                token = NextToken();
            }

            #region Process mismatches

            foreach (Token mismatch in mismatchingPairs) {
                BlameType errorType;
                switch (mismatch.Type) {
                    case LeftParenthesis:
                    case RightParenthesis: {
                        errorType = BlameType.MismatchedParenthesis;
                        break;
                    }
                    case LeftBracket:
                    case RightBracket: {
                        errorType = BlameType.MismatchedBracket;
                        break;
                    }
                    case LeftBrace:
                    case RightBrace: {
                        errorType = BlameType.MismatchedBrace;
                        break;
                    }
                    default: {
                        throw new NotSupportedException(
                            "Internal error: "
                            + nameof(mismatchingPairs)
                            + " grabbed invalid "
                            + nameof(TokenType)
                            + ": "
                            + mismatch.Type
                        );
                    }
                }

                unit.Blame(errorType, mismatch.Span.StartPosition, mismatch.Span.EndPosition);
            }

            #endregion
        }

        private void AddPresets(
            List<MultilineCommentToken> unclosedMultilineComments_ = null,
            List<StringToken>           unclosedStrings_           = null,
            string[]                    processTerminators_        = null
        ) {
            unclosedStrings =
                unclosedStrings_ ?? new List<StringToken>();
            unclosedMultilineComments =
                unclosedMultilineComments_ ?? new List<MultilineCommentToken>();
            processingTerminators =
                processTerminators_ ?? new string[0];
        }

        /// <summary>
        /// <c>
        ///     Reads next token from character stream.
        ///     Every time that method invoked, CharStream.C
        ///     property should be on first letter of current new reading token.
        /// </c>
        /// </summary>
        private Token NextToken() {
            // reset token properties
            tokenStartPosition = stream.Position;
            tokenValue.Clear();

#if DEBUG
            if (tokens.Count != 0
                && tokens[tokens.Count - 1].Type != Outdent) {
                Token last = tokens[tokens.Count - 1];
                Debug.Assert(
                    tokenStartPosition
                    == (last.Span.EndPosition.Line,
                        last.Span.EndPosition.Column + last.Whitespaces.Length)
                );
            }
#endif

            if (c == EndOfStream) {
                return new EndOfCodeToken(tokenStartPosition);
            }

            if (c == '\r') {
                stream.Move();
                if (c == '\n') {
                    tokenValue.Append('\r');
                }
                else {
                    return null;
                }
            }

            // this branch should forever be
            // right after \r check.
            if (c == '\n') {
                return ReadNewline();
            }

            if (IsSpaceOrTab(c)) {
                return ReadWhite();
            }

            if (c.ToString() == SingleCommentStart) {
                // one-line comment
                if (c.ToString() + stream.Peek == MultiCommentStart) {
                    return ReadMultilineComment();
                }

                return ReadSingleLineComment();
            }

            if (c == CharacterLiteralQuote) {
                return ReadCharLiteral();
            }

            if (StringQuotes.Contains(c)) {
                return ReadString(false);
            }

            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            if (IsValidIdStart(c)) {
                return ReadWord();
            }

            if (SymbolicChars.Contains(c)) {
                return ReadSymbolic();
            }

            // invalid
            tokenValue.Append(c);
            stream.Move();
            unit.Blame(BlameType.InvalidSymbol, tokenStartPosition, stream.Position);
            return new Token(Invalid, tokenValue.ToString(), tokenStartPosition);
        }
    }
}