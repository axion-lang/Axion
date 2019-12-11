using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic {
    public class TokenStream {
        public readonly List<Token> Tokens = new List<Token>();
        public int TokenIdx { get; private set; } = -1;

        public Token Token =>
            TokenIdx > -1 && TokenIdx < Tokens.Count
                ? Tokens[TokenIdx]
                : Tokens[^1];

        private Token exactPeek =>
            TokenIdx + 1 < Tokens.Count
                ? Tokens[TokenIdx     + 1]
                : Tokens[^1];

        public Token Peek {
            get {
                SkipTrivial();
                return exactPeek;
            }
        }

        public bool PeekIs(params TokenType[] expected) {
            return PeekByIs(1, expected);
        }

        public bool PeekByIs(int peekBy, params TokenType[] expected) {
            SkipTrivial(expected);
            if (TokenIdx + peekBy < Tokens.Count) {
                Token peekN = Tokens[TokenIdx + peekBy];
                for (var i = 0; i < expected.Length; i++) {
                    if (peekN.Is(expected[i])) {
                        return true;
                    }
                }
            }

            return false;
        }

        public Token EatAny(int pos = 1) {
            if (TokenIdx + pos >= 0
             && TokenIdx + pos < Tokens.Count) {
                TokenIdx += pos;
            }

            return Tokens[TokenIdx];
        }

        /// <summary>
        ///     Skips new line token, failing,
        ///     if the next token type is not
        ///     the same as passed in parameter.
        /// </summary>
        public Token Eat(params TokenType[] types) {
            SkipTrivial(types);
            EatAny();
            if (Token.Is(types)) {
                return Token;
            }

            LangException.Report(BlameType.InvalidSyntax, Token);
            return null;
        }

        /// <summary>
        ///     Skips token of specified type,
        ///     returns: was token skipped or not.
        /// </summary>
        public bool MaybeEat(params TokenType[] types) {
            SkipTrivial(types);
            for (var i = 0; i < types.Length; i++) {
                if (exactPeek.Is(types[i])) {
                    EatAny();
                    return true;
                }
            }

            return false;
        }

        public void MoveAbsolute(int tokenIndex) {
            Debug.Assert(tokenIndex >= -1 && tokenIndex < Tokens.Count);
            TokenIdx = tokenIndex;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (true) {
                if (exactPeek.Is(Comment)) {
                    EatAny();
                }
                // if we got newline before wanted type, just skip it
                // (except we WANT to get newline)
                else if (exactPeek.Is(Newline)
                      && !wantedTypes.Contains(Newline)) {
                    EatAny();
                }
                else {
                    break;
                }
            }
        }
    }
}