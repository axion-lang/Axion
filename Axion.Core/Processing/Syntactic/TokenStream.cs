using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic {
    [DebuggerDisplay("{TokenIdx}: '{Token.Value}', then '{Peek.Value}'.")]
    public class TokenStream {
        public List<Token> Tokens   { get; }              = new List<Token>();
        public int         TokenIdx { get; private set; } = -1;

        public Token Token =>
            TokenIdx > -1 && TokenIdx < Tokens.Count ? Tokens[TokenIdx] : Tokens[^1];

        public Token ExactPeek =>
            TokenIdx + 1 < Tokens.Count ? Tokens[TokenIdx + 1] : Tokens[^1];

        public Token Peek {
            get {
                SkipTrivial();
                return ExactPeek;
            }
        }

        public bool PeekIs(params TokenType[] expected) {
            return PeekByIs(1, expected);
        }

        public bool PeekByIs(int peekBy, params TokenType[] expected) {
            SkipTrivial(expected);
            if (TokenIdx + peekBy < Tokens.Count) {
                Token peekN = Tokens[TokenIdx + peekBy];
                foreach (TokenType t in expected) {
                    if (peekN.Is(t)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public Token EatAny(int pos = 1) {
            if (TokenIdx + pos >= 0 && TokenIdx + pos < Tokens.Count) {
                TokenIdx += pos;
            }

            return Tokens[TokenIdx];
        }

        /// <summary>
        ///     Skips and returns next token.
        /// </summary>
        public Token Eat() {
            SkipTrivial();
            EatAny();
            return Token;
        }

        /// <summary>
        ///     Skips next token, failing,
        ///     if the next token type is not
        ///     the same as passed in parameter.
        /// </summary>
        public Token? Eat(params TokenType[] types) {
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
                if (ExactPeek.Is(types[i])) {
                    EatAny();
                    return true;
                }
            }

            return false;
        }

        public bool MaybeEat(string value) {
            SkipTrivial();
            if (ExactPeek.Value != value) {
                return false;
            }
            EatAny();
            return true;
        }

        public void MoveAbsolute(int tokenIndex) {
            Debug.Assert(tokenIndex >= -1 && tokenIndex < Tokens.Count);
            TokenIdx = tokenIndex;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            bool skipNewlines = !wantedTypes.Contains(Newline);
            while (ExactPeek.Type == Comment || ExactPeek.Type == Newline && skipNewlines) {
                EatAny();
            }
        }
    }
}
