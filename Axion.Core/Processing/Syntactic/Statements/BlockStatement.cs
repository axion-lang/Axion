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
                    bool isInLoop        = Ast.InLoop,
                         isInFinally     = Ast.InFinally,
                         isInFinallyLoop = Ast.InFinallyLoop;
                    try {
                        Ast.InLoop        = false;
                        Ast.InFinally     = false;
                        Ast.InFinallyLoop = false;
                        Parse(parent);
                    }
                    finally {
                        Ast.InLoop        = isInLoop;
                        Ast.InFinally     = isInFinally;
                        Ast.InFinallyLoop = isInFinallyLoop;
                    }

                    break;
                }

                case BlockType.Loop: {
                    bool wasInLoop        = Ast.InLoop,
                         wasInFinallyLoop = Ast.InFinallyLoop;
                    try {
                        Ast.InLoop        = true;
                        Ast.InFinallyLoop = Ast.InFinally;
                        Parse(parent);
                    }
                    finally {
                        Ast.InLoop        = wasInLoop;
                        Ast.InFinallyLoop = wasInFinallyLoop;
                    }

                    break;
                }

                case BlockType.Anyway: {
                    if (Ast.CurrentFunction != null) {
                        Ast.CurrentFunction.ContainsTryFinally = true;
                    }

                    bool isInFinally     = Ast.InFinally,
                         isInFinallyLoop = Ast.InFinallyLoop;
                    try {
                        Ast.InFinally     = true;
                        Ast.InFinallyLoop = false;
                        Parse(parent);
                    }
                    finally {
                        Ast.InFinally     = isInFinally;
                        Ast.InFinallyLoop = isInFinallyLoop;
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