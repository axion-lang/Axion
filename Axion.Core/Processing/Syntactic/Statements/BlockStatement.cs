using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         block ::=
    ///               (':' stmt)
    ///             | ('{' stmt+ '}')
    ///             | (':' NEWLINE INDENT stmt+ OUTDENT)
    ///     </c>
    /// </summary>
    public class BlockStatement : Statement {
        private NodeList<Statement> statements;

        [NotNull]
        public NodeList<Statement> Statements {
            get => statements;
            set => SetNode(ref statements, value);
        }

        internal BlockStatement([NotNull] IEnumerable<Statement> statements) {
            Statements = new NodeList<Statement>(this, statements);

            if (Statements.Count != 0) {
                MarkPosition(Statements.First, Statements.Last);
            }
        }

        internal BlockStatement(SyntaxTreeNode parent, BlockType type = BlockType.Default) {
            Parent = parent;
            switch (type) {
                case BlockType.Top: {
                    bool isInLoop        = Unit.Ast.InLoop,
                         isInFinally     = Unit.Ast.InFinally,
                         isInFinallyLoop = Unit.Ast.InFinallyLoop;
                    try {
                        Unit.Ast.InLoop        = false;
                        Unit.Ast.InFinally     = false;
                        Unit.Ast.InFinallyLoop = false;
                        Parse(parent);
                    }
                    finally {
                        Unit.Ast.InLoop        = isInLoop;
                        Unit.Ast.InFinally     = isInFinally;
                        Unit.Ast.InFinallyLoop = isInFinallyLoop;
                    }

                    break;
                }

                case BlockType.Loop: {
                    bool wasInLoop        = Unit.Ast.InLoop,
                         wasInFinallyLoop = Unit.Ast.InFinallyLoop;
                    try {
                        Unit.Ast.InLoop        = true;
                        Unit.Ast.InFinallyLoop = Unit.Ast.InFinally;
                        Parse(parent);
                    }
                    finally {
                        Unit.Ast.InLoop        = wasInLoop;
                        Unit.Ast.InFinallyLoop = wasInFinallyLoop;
                    }

                    break;
                }

                case BlockType.Anyway: {
                    if (Unit.Ast.currentFunction != null) {
                        Unit.Ast.currentFunction.ContainsTryFinally = true;
                    }

                    bool isInFinally     = Unit.Ast.InFinally,
                         isInFinallyLoop = Unit.Ast.InFinallyLoop;
                    try {
                        Unit.Ast.InFinally     = true;
                        Unit.Ast.InFinallyLoop = false;
                        Parse(parent);
                    }
                    finally {
                        Unit.Ast.InFinally     = isInFinally;
                        Unit.Ast.InFinallyLoop = isInFinallyLoop;
                    }

                    break;
                }

                default: {
                    Parse(parent);
                    break;
                }
            }
        }

        private void Parse(SyntaxTreeNode parent, bool usesColon = true) {
            Parent     = parent;
            Statements = new NodeList<Statement>(this);
            (TokenType terminator, bool oneLine, bool error)
                = ParseStart(this, usesColon);

            if (oneLine) {
                Statements.Add(ParseStmt(this));
            }
            else if (!error && !MaybeEat(terminator)) {
                while (true) {
                    Statements.Add(ParseStmt(this, terminator));
                    if (MaybeEat(terminator)) {
                        break;
                    }

                    if (PeekIs(TokenType.End)) {
                        // report error for unclosed block, if it is not outdent.
                        if (terminator != TokenType.Outdent) {
                            CheckUnexpectedEoc();
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     Starts parsing the statement's block,
        ///     returns terminator what can be used to parse block end.
        /// </summary>
        internal static (TokenType terminator, bool oneLine, bool error) ParseStart(
            SyntaxTreeNode parent,
            bool           usesColon = true
        ) {
            // 1) colon
            bool  hasColon   = parent.MaybeEat(TokenType.Colon);
            Token blockStart = parent.Token;

            // 1-2) newline
            bool hasNewline;
            hasNewline = hasColon
                ? parent.MaybeEatNewline()
                : blockStart.Is(TokenType.Newline);

            TokenType terminator;
            // 1-3) '{'
            if (parent.MaybeEat(TokenType.OpenBrace)) {
                terminator = TokenType.CloseBrace;
            }
            // 3) INDENT
            else if (parent.MaybeEat(TokenType.Indent)) {
                terminator = TokenType.Outdent;
            }
            else {
                // no indent or brace? - report block error
                if (hasNewline) {
                    // newline but with invalid follower
                    parent.Unit.Blame(BlameType.ExpectedBlockDeclaration, parent.Peek);
                    return (TokenType.None, false, true);
                }

                // no newline
                if (!hasColon && usesColon) {
                    // must have a colon
                    parent.BlameInvalidSyntax(TokenType.Colon, parent.Peek);
                    return (TokenType.None, true, true);
                }

                return (TokenType.Newline, true, false);
            }

            // ':' followed by '{'
            if (hasColon && terminator == TokenType.CloseBrace) {
                parent.Unit.Blame(BlameType.RedundantColonWithBraces, blockStart);
            }

            parent.MaybeEatNewline();
            return (terminator, false, false);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c += "{ ";
            c.AppendJoin("; ", Statements);
            return c + "; }";
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c += "{";
            c.AppendJoin("", Statements);
            return c + "}";
        }
    }

    public enum BlockType {
        Default,
        Top,
        Loop,
        Anyway
    }
}