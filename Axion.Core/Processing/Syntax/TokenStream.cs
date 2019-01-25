using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Parser;

namespace Axion.Core.Processing.Syntax {
    public class TokenStream {
        internal readonly string Source;

        /// <summary>
        ///     Reference to processing <see cref="List{T}" /> of tokens.
        /// </summary>
        internal readonly List<Token> Tokens;

        internal Token Token => Index > -1 && Index < Tokens.Count ? Tokens[Index] : Tokens[Tokens.Count - 1];

        internal Token Peek => Index + 1 < Tokens.Count ? Tokens[Index + 1] : Tokens[Tokens.Count - 1];

        internal int Index { get; private set; } = -1;

        private readonly SyntaxParser parser;

        public TokenStream(SyntaxParser parser, List<Token> tokens, string source) {
            this.parser = parser;
            Tokens      = tokens;
            Source      = source;
        }

        public bool PeekIs(TokenType expected) {
            return Peek.Type == expected;
        }

        public bool PeekIs(params TokenType[] expected) {
            for (var i = 0; i < expected.Length; i++) {
                if (Peek.Type == expected[i]) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Maybe eats a new line token returning true if the token was eaten.
        /// </summary>
        internal bool MaybeEatNewline() {
            return MaybeEat(TokenType.Newline);
        }

        /// <summary>
        ///     Eats a new line token throwing if the next token isn't a new line.
        /// </summary>
        internal bool EatNewline() {
            return Eat(TokenType.Newline);
        }

        internal bool Eat(TokenType type) {
            SkipTrivial(type);
            if (Peek.Type != type) {
                parser.BlameInvalidSyntax(type, Peek);
                return false;
            }
            Move();
            return true;
        }

        internal bool MaybeEat(TokenType type) {
            SkipTrivial(type);
            if (Peek.Type == type) {
                Move();
                return true;
            }
            return false;
        }

        internal bool MaybeEat(params TokenType[] types) {
            SkipTrivial(types);
            for (var i = 0; i < types.Length; i++) {
                if (Peek.Type == types[i]) {
                    Move();
                    return true;
                }
            }
            return false;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (PeekIs(TokenType.Comment)) {
                Move();
            }

            // if we got newline before wanted type, just skip it
            // (except we WANT to get newline)
            if (Peek.Type == TokenType.Newline
             && !wantedTypes.Contains(TokenType.Newline)) {
                Move();
            }
        }

        public Token NextToken() {
            Move();
            return Token;
        }

        private void Move(int pos = 1) {
            Index += pos;
        }

        internal void MoveTo(int tokenIndex) {
            Debug.Assert(tokenIndex >= 0 && tokenIndex < Tokens.Count);
            Index = tokenIndex;
        }
    }
}