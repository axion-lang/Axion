using System;
using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic {
    public abstract class SyntaxTreeNode : SpannedRegion {
        protected internal SyntaxTreeNode Parent;
        private            Ast            ast;

        internal Ast Ast {
            get {
                if (ast == null) {
                    SyntaxTreeNode p = this;
                    while (!(p is Ast)) {
                        p = p.Parent;
                    }

                    ast = (Ast) p;
                }

                return ast;
            }
        }

        internal virtual TypeName ValueType => throw new NotSupportedException();

        internal SyntaxTreeNode(SyntaxTreeNode parent) {
            Parent = parent;
        }

        protected SyntaxTreeNode() { }

        public override string ToString() {
            var code = new CodeBuilder(OutLang.Axion);
            ToAxionCode(code);
            return code.ToString();
        }

        #region AST properties short links

        internal SourceUnit Unit => Ast.SourceUnit;

        internal Token Token =>
            Ast.Index > -1
            && Ast.Index < Unit.Tokens.Count
                ? Unit.Tokens[Ast.Index]
                : Unit.Tokens[Unit.Tokens.Count - 1];

        private Token exactPeek =>
            Ast.Index + 1 < Unit.Tokens.Count
                ? Unit.Tokens[Ast.Index + 1]
                : Unit.Tokens[Unit.Tokens.Count - 1];

        internal Token Peek {
            get {
                SkipTrivial();
                return exactPeek;
            }
        }

        #endregion

        #region Syntax node's property setter helpers

        protected void SetNode<T>(ref T field, T value) where T : SyntaxTreeNode? {
            if (field == value) {
                return;
            }

            if (value != null) {
                value.Parent = this;
            }

            field = value;
        }

        protected void SetNode<T>(ref NodeList<T> field, NodeList<T> value)
            where T : SyntaxTreeNode? {
            if (field == value) {
                return;
            }

            if (value != null) {
                foreach (T expr in value) {
                    if (expr != null) {
                        expr.Parent = this;
                    }
                }
            }
            else {
                value = new NodeList<T>(this);
            }

            field = value;
        }

        #endregion

        #region Errors reporting

        internal void BlameInvalidSyntax(TokenType expectedType, SpannedRegion mark) {
            Unit.ReportError(
                "Invalid syntax, expected '"
                + expectedType.GetValue()
                + "', got '"
                + exactPeek.Type.GetValue()
                + "'.",
                mark
            );
        }

        #endregion

        #region Token stream controllers

        /// <summary>
        ///     Eats specified <paramref name="type"/>
        ///     and marks current token as node start.
        /// </summary>
        internal Token MarkStart(TokenType type) {
            Eat(type);
            MarkStart(Token);
            return Token;
        }

        internal bool PeekByIs(int peekBy, params TokenType[] expected) {
            SkipTrivial(expected);
            if (Ast.Index + peekBy < Unit.Tokens.Count) {
                Token peekN = Unit.Tokens[Ast.Index + peekBy];
                for (var i = 0; i < expected.Length; i++) {
                    if (peekN.Is(expected[i])) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Adjusts [<see cref="Ast"/>.Index] by [<paramref name="pos"/>]
        ///     and returns token by current index.
        /// </summary>
        internal Token Move(int pos = 1) {
            if (Ast.Index + pos >= 0
                && Ast.Index + pos < Unit.Tokens.Count) {
                Ast.Index += pos;
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
                Move();
            }

            return matches;
        }

        /// <summary>
        ///     Skips token of specified type,
        ///     returns: was token skipped or not.
        /// </summary>
        internal bool MaybeEat(TokenType type) {
            SkipTrivial(type);
            if (exactPeek.Is(type)) {
                Move();
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
                if (exactPeek.Is(types[i])) {
                    Move();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Moves current token index by absolute value.
        /// </summary>
        internal void MoveTo(int tokenIndex) {
            Debug.Assert(tokenIndex >= -1 && tokenIndex < Unit.Tokens.Count);
            Ast.Index = tokenIndex;
        }

        internal bool EnsureNext(params TokenType[] expected) {
            SkipTrivial(expected);
            for (var i = 0; i < expected.Length; i++) {
                if (exactPeek.Is(expected[i])) {
                    return true;
                }
            }

            BlameInvalidSyntax(expected[0], exactPeek);
            return false;
        }

        private void SkipTrivial(params TokenType[] wantedTypes) {
            while (true) {
                if (exactPeek.Is(TokenType.Comment)) {
                    Move();
                }
                // if we got newline before wanted type, just skip it
                // (except we WANT to get newline)
                else if (exactPeek.Is(TokenType.Newline)
                         && !wantedTypes.Contains(TokenType.Newline)) {
                    Move();
                }
                else {
                    break;
                }
            }
        }

        #endregion
    }
}