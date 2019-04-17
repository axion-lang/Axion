using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical {
    /// <summary>
    ///     Tool for splitting Axion
    ///     code into list of tokens.
    /// </summary>
    public partial class Lexer {
        /// <summary>
        ///     Current processing source unit.
        /// </summary>
        private readonly SourceUnit unit;

        /// <summary>
        ///     Resulting tokens list.
        /// </summary>
        private readonly List<Token> tokens;

        /// <summary>
        ///     All unclosed multiline comments found in source.
        /// </summary>
        private readonly List<CommentToken> unclosedMultilineComments = new List<CommentToken>();

        /// <summary>
        ///     All unclosed strings found in source.
        /// </summary>
        private readonly List<StringToken> unclosedStrings = new List<StringToken>();

        /// <summary>
        ///     All unpaired '{', '[', '(' found in source.
        /// </summary>
        private readonly List<Token> mismatchingPairs = new List<Token>();

        /// <summary>
        ///     Values, from which lexer should stop working.
        /// </summary>
        private readonly List<string> processCancellers = new List<string>();

        #region Current token properties

        /// <summary>
        ///     Value of current reading token.
        /// </summary>
        private readonly StringBuilder tokenValue = new StringBuilder();

        /// <summary>
        ///     Start position of current reading token.
        /// </summary>
        private Position tokenStartPosition = (0, 0);

        #endregion

        public Lexer(SourceUnit sourceUnit) {
            unit   = sourceUnit;
            tokens = sourceUnit.Tokens;
        }

        /// <summary>
        ///     Private constructor for
        ///     string interpolations reading.
        /// </summary>
        private Lexer(
            SourceUnit  sourceUnit,
            int         charI,
            int         lineI,
            int         columnI,
            List<Token> outTokens
        ) {
            unit      = sourceUnit;
            charIdx   = charI;
            lineIdx   = lineI;
            columnIdx = columnI;
            tokens    = outTokens ?? new List<Token>();
        }

        /// <summary>
        ///     Divides code into list of tokens.
        /// </summary>
        public void Process() {
            if (string.IsNullOrWhiteSpace(unit.Code)) {
                return;
            }

            Token? token = null;
            // at first, check if we're after unclosed string
            if (unclosedStrings.Count > 0) {
                token = ReadString(true);
            }

            // then, check if we're after unclosed multiline comment
            if (unclosedMultilineComments.Count > 0) {
                token = ReadMultilineComment();
            }

            if (token == null) {
                token = ReadNextToken();
            }

            while (true) {
                if (token != null) {
                    tokens.Add(token);
                    // check for processing terminator
                    if (token.Is(TokenType.End) || processCancellers.Contains(token.Value)) {
                        break;
                    }
                }

                token = ReadNextToken();
            }

            #region Process mismatches

            foreach (Token mismatch in mismatchingPairs) {
                BlameType errorType;
                switch (mismatch.Type) {
                    case TokenType.OpenParenthesis:
                    case TokenType.CloseParenthesis: {
                        errorType = BlameType.MismatchedParenthesis;
                        break;
                    }

                    case TokenType.OpenBracket:
                    case TokenType.CloseBracket: {
                        errorType = BlameType.MismatchedBracket;
                        break;
                    }

                    case TokenType.OpenBrace:
                    case TokenType.CloseBrace: {
                        errorType = BlameType.MismatchedBrace;
                        break;
                    }

                    default: {
                        throw new Exception(
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

        private Token? ReadNextToken() {
            // reset token properties
            tokenStartPosition = Position;
            tokenValue.Clear();

#if DEBUG
            if (tokens.Count != 0) {
                Token last = tokens.Last();
                if (last.Type != TokenType.Outdent) {
                    Debug.Assert(
                        tokenStartPosition
                        == (last.Span.EndPosition.Line,
                            last.Span.EndPosition.Column + last.EndWhitespaces.Length)
                    );
                }
            }
#endif

            if (c == Spec.EndOfCode) {
                return new Token(TokenType.End, tokenStartPosition);
            }

            if (c == '\r') {
                Move();
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

            if (c.IsSpaceOrTab()) {
                return ReadWhite();
            }

            if (c.ToString() == Spec.CommentStart) {
                // one-line comment
                if (c.ToString() + Peek == Spec.MultiCommentStart) {
                    return ReadMultilineComment();
                }

                return ReadSingleLineComment();
            }

            if (c == Spec.CharacterLiteralQuote) {
                return ReadCharLiteral();
            }

            if (Spec.StringQuotes.Contains(c)) {
                return ReadString(false);
            }

            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            if (c.IsValidIdStart()) {
                return ReadWord();
            }

            if (Spec.SymbolicChars.Contains(c)) {
                return ReadSymbolic();
            }

            // invalid
            tokenValue.Append(c);
            Move();
            unit.Blame(BlameType.InvalidCharacter, tokenStartPosition, Position);
            return new Token(TokenType.Invalid, tokenValue.ToString(), tokenStartPosition);
        }
    }
}