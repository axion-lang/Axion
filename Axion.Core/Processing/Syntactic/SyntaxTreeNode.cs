using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntactic {
    public class SyntaxTreeNode : SpannedRegion {
        [JsonIgnore]
        protected internal SyntaxTreeNode Parent;

        internal Ast Ast {
            get {
                SyntaxTreeNode p = this;
                while (!(p is Ast)) {
                    p = p.Parent;
                }

                return (Ast) p;
            }
        }

        // short links
        internal SourceUnit Unit   => Ast.SourceUnit;
        internal int        _Index => Ast._Index;
        internal Token      Token  => Ast.Token;
        internal Token      Peek   => Ast.Peek;

        protected bool SetNode<T>(ref T field, T value) where T : SyntaxTreeNode {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            if (value != null) {
                value.Parent = this;
            }

            field = value;
            return true;
        }

        protected bool SetNode<T>(ref T[] field, T[] value) where T : SyntaxTreeNode {
            if (field == value) {
                return false;
            }

            if (value != null) {
                foreach (T expr in value) {
                    expr.Parent = this;
                }
            }

            field = value;
            return true;
        }

        protected bool SetNode<T>(ref NodeList<T> field, NodeList<T> value)
            where T : SyntaxTreeNode {
            if (field == value) {
                return false;
            }

            if (value != null) {
                foreach (T expr in value) {
                    expr.Parent = this;
                }
            }
            else {
                value = new NodeList<T>(this);
            }

            field = value;
            return true;
        }

        internal Token StartNode(TokenType type) {
            Eat(type);
            MarkStart(Token);
            return Token;
        }

        public bool PeekIs(TokenType expected) {
            return Peek.Is(expected);
        }

        internal bool PeekIs(params TokenType[] expected) {
            SkipTrivial(expected);
            for (var i = 0; i < expected.Length; i++) {
                if (Peek.Is(expected[i])) {
                    return true;
                }
            }

            return false;
        }

        private bool EnsureNext(params TokenType[] expected) {
            SkipTrivial(expected);
            for (var i = 0; i < expected.Length; i++) {
                if (Peek.Is(expected[i])) {
                    return true;
                }
            }

            BlameInvalidSyntax(expected[0], Peek);
            return false;
        }

        internal Token NextToken(int pos = 1) {
            if (Ast._Index + pos >= 0
                && Ast._Index + pos < Unit.Tokens.Count) {
                Ast._Index += pos;
            }

            return Token;
        }

        /// <summary>
        ///     Skips new line token, failing,
        ///     if the next token type is not
        ///     the same as passed in parameter.
        /// </summary>
        internal bool Eat(TokenType type) {
            bool matches = EnsureNext(type);
            if (matches) {
                NextToken();
            }

            return matches;
        }

        /// <summary>
        ///     Skips token of specified type,
        ///     returns: was token skipped or not.
        /// </summary>
        internal bool MaybeEat(TokenType type) {
            SkipTrivial(type);
            if (Peek.Is(type)) {
                NextToken();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Skips new line token,
        ///     returns: was token skipped or not.
        /// </summary>
        internal bool MaybeEatNewline() {
            return MaybeEat(TokenType.Newline);
        }

        internal bool MaybeEat(params TokenType[] types) {
            SkipTrivial(types);
            for (var i = 0; i < types.Length; i++) {
                if (Peek.Is(types[i])) {
                    NextToken();
                    return true;
                }
            }

            return false;
        }

        internal void MoveTo(int tokenIndex) {
            Debug.Assert(tokenIndex >= 0 && tokenIndex < Unit.Tokens.Count);
            Ast._Index = tokenIndex;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (true) {
                if (PeekIs(TokenType.Comment)) {
                    NextToken();
                }
                // if we got newline before wanted type, just skip it
                // (except we WANT to get newline)
                else if (Peek.Is(TokenType.Newline) && !wantedTypes.Contains(TokenType.Newline)) {
                    NextToken();
                }
                else {
                    break;
                }
            }
        }

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            Unit.ReportError(
                "Invalid syntax, expected '"
                + expectedType.GetValue()
                + "', got '"
                + Peek.Type.GetValue()
                + "'.",
                mark
            );
        }

        public bool CheckUnexpectedEoc() {
            if (!PeekIs(TokenType.End)) {
                return false;
            }

            Unit.Blame(BlameType.UnexpectedEndOfCode, Token);
            return true;
        }
    }
}