using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Parser;

namespace Axion.Core.Processing.Syntax {
    public class TokenStream {
        /// <summary>
        ///     Reference to processing <see cref="List{T}" /> of tokens.
        /// </summary>
        internal readonly List<Token> Tokens;

        private readonly ParserBase parser;

        private bool inOneLineMode;

        public TokenStream(ParserBase parser, List<Token> tokens) {
            this.parser = parser;
            Tokens      = tokens;
        }

        internal Token Token =>
            Index > -1 && Index < Tokens.Count ? Tokens[Index] : Tokens[Tokens.Count - 1];

        internal Token Peek =>
            Index + 1 < Tokens.Count ? Tokens[Index + 1] : Tokens[Tokens.Count - 1];

        internal int Index { get; private set; } = -1;

        public bool PeekIs(TokenType expected) {
            return Peek.Type == expected;
        }

        public bool PeekIs(params TokenType[] expected) {
            SkipTrivial(expected);
            for (var i = 0; i < expected.Length; i++) {
                if (Peek.Type == expected[i]) {
                    return true;
                }
            }

            return false;
        }

        public bool EnsureNext(TokenType type) {
            SkipTrivial(type);
            if (Peek.Type != type) {
                parser.BlameInvalidSyntax(type, Peek);
                return false;
            }
            return true;
        }

        public void EnsureOneLine(Action action) {
            inOneLineMode = true;
            action();
            inOneLineMode = false;
        }

        public Token NextToken(int pos = 1) {
            if (Index + pos >= 0 && Index + pos < Tokens.Count) {
                Index += pos;
            }

            return Token;
        }

        internal bool Eat(TokenType type) {
            bool matches = EnsureNext(type);
            if (matches) {
                NextToken();
            }
            return matches;
        }

        /// <summary>
        ///     Eats a new line token throwing if the next token isn't a new line.
        /// </summary>
        internal bool EatNewline() {
            return Eat(TokenType.Newline);
        }

        internal bool MaybeEat(TokenType type) {
            SkipTrivial(type);
            if (Peek.Type == type) {
                NextToken();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Maybe eats a new line token returning true if the token was eaten.
        /// </summary>
        internal bool MaybeEatNewline() {
            return MaybeEat(TokenType.Newline);
        }

        internal bool MaybeEat(params TokenType[] types) {
            SkipTrivial(types);
            for (var i = 0; i < types.Length; i++) {
                if (Peek.Type == types[i]) {
                    NextToken();
                    return true;
                }
            }
            return false;
        }

        internal void MoveTo(int tokenIndex) {
            Debug.Assert(tokenIndex >= 0 && tokenIndex < Tokens.Count);
            Index = tokenIndex;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (true) {
                if (PeekIs(TokenType.Comment)) {
                    NextToken();
                }
                // if we got newline before wanted type, just skip it
                // (except we WANT to get newline)
                else if (Peek.Type == TokenType.Newline
                      && !wantedTypes.Contains(TokenType.Newline)) {
                    if (inOneLineMode) {
                        throw new NotSupportedException(
                            "Syntax error: got newline in one-line expression."
                        );
                    }
                    NextToken();
                }
                else {
                    break;
                }
            }
        }
    }
}