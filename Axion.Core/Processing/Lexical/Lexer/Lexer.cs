using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Lexer {
    /// <summary>
    ///     Static tool for splitting Axion
    ///     code into list of tokens.
    /// </summary>
    public partial class Lexer : AbstractLexer {
        #region Current source properties

        private char c => Stream.C;

        /// <summary>
        ///     Contains values, from which lexer should stop working.
        /// </summary>
        private string[] processingTerminators;

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
        private Position tokenStartPosition = (0, 0);

        #endregion

        #region Constructors

        public Lexer(
            string                  codeToProcess,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) : base(codeToProcess, outTokens, outBlames, processingOptions) {
            AddPresets();
        }

        public Lexer(
            CharStream              fromStream,
            List<Token>             outTokens,
            List<Exception>         outBlames,
            SourceProcessingOptions processingOptions = SourceProcessingOptions.None
        ) : base(fromStream, outTokens, outBlames, processingOptions) {
            AddPresets();
        }

        #endregion

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
                    tokens.Add(token);
                    // check for processing terminator
                    if (token.Type == TokenType.EndOfCode
                     || processingTerminators.Contains(token.Value)) {
                        break;
                    }
                }
                token = NextToken();
            }

            #region Process mismatches

            foreach (Token mismatch in _mismatchingPairs) {
                BlameType errorType;
                switch (mismatch.Type) {
                    case TokenType.LeftParenthesis:
                    case TokenType.RightParenthesis: {
                        errorType = BlameType.MismatchedParenthesis;
                        break;
                    }
                    case TokenType.LeftBracket:
                    case TokenType.RightBracket: {
                        errorType = BlameType.MismatchedBracket;
                        break;
                    }
                    case TokenType.LeftBrace:
                    case TokenType.RightBrace: {
                        errorType = BlameType.MismatchedBrace;
                        break;
                    }
                    default: {
                        throw new NotSupportedException(
                            "Internal error: "
                          + nameof(_mismatchingPairs)
                          + " grabbed invalid "
                          + nameof(TokenType)
                          + ": "
                          + mismatch.Type
                        );
                    }
                }
                Blame(errorType, mismatch.Span.StartPosition, mismatch.Span.EndPosition);
            }

            #endregion
        }

        protected sealed override void AddPresets(
            List<MultilineCommentToken> unclosedMultilineComments = null,
            List<StringToken>           unclosedStrings           = null,
            string[]                    processTerminators        = null
        ) {
            _unclosedStrings = unclosedStrings ?? new List<StringToken>();
            _unclosedMultilineComments =
                unclosedMultilineComments ?? new List<MultilineCommentToken>();
            processingTerminators = processTerminators ?? new string[0];
        }

        /// <summary>
        ///     Reads next token from character stream.
        ///     <para />
        ///     Every time that method invoked, <see cref="CharStream.C" />
        ///     property should be on first letter of current new reading token.
        /// </summary>
        private Token NextToken() {
            // reset token properties
            tokenStartPosition = Stream.Position;
            tokenValue.Clear();

#if DEBUG
            if (tokens.Count != 0 && tokens[tokens.Count - 1].Type != TokenType.Outdent) {
                Token last = tokens[tokens.Count - 1];
                Debug.Assert(
                    tokenStartPosition
                 == (last.Span.EndPosition.Line,
                     last.Span.EndPosition.Column + last.Whitespaces.Length)
                );
            }
#endif

            if (c == Spec.EndOfStream) {
                return new EndOfCodeToken(tokenStartPosition);
            }

            if (c == '\r') {
                Stream.Move();
                if (c == '\n') {
                    tokenValue.Append('\r');
                }
                else {
                    // skip carriage returns
                    return null;
                }
            }
            // this branch should forever be
            // right after \r check.
            if (c == '\n') {
                return ReadNewline();
            }

            if (Spec.IsSpaceOrTab(c)) {
                // whitespaces & indentation
                return ReadWhite();
            }

            if (c.ToString() == Spec.SingleCommentStart) {
                // one-line comment
                if (c.ToString() + Stream.Peek == Spec.MultiCommentStart) {
                    // multiline comment
                    return ReadMultilineComment();
                }
                return ReadSingleLineComment();
            }

            if (c == Spec.CharLiteralQuote) {
                return ReadCharLiteral();
            }

            if (Spec.StringQuotes.Contains(c)) {
                return ReadString(false);
            }

            if (char.IsDigit(c)) {
                return ReadNumber();
            }

            if (Spec.IsValidIdStart(c)) {
                return ReadWord();
            }

            if (Spec.SymbolicChars.Contains(c)) {
                return ReadSymbolic();
            }

            // invalid
            tokenValue.Append(c);
            Stream.Move();
            Blame(BlameType.InvalidSymbol, tokenStartPosition, Stream.Position);
            return new Token(TokenType.Invalid, tokenStartPosition, tokenValue.ToString());
        }
    }
}