using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements {
    /// <summary>
    ///     <c>
    ///         block:
    ///               (':' stmt)
    ///             | ([':'] '{' stmt* '}')
    ///             | ([':'] NEWLINE INDENT stmt+ OUTDENT)
    ///     </c>
    /// </summary>
    public class BlockStatement : Statement {
        private NodeList<Statement> statements;

        public NodeList<Statement> Statements {
            get => statements;
            set => SetNode(ref statements, value);
        }

        public readonly List<VariableDefinitionExpression> Variables =
            new List<VariableDefinitionExpression>();

        public readonly List<FunctionDefinition> Functions = new List<FunctionDefinition>();
        public readonly List<ClassDefinition>    Classes   = new List<ClassDefinition>();
        public readonly List<EnumDefinition>     Enums     = new List<EnumDefinition>();
        public readonly List<ModuleDefinition>   Modules   = new List<ModuleDefinition>();

        #region Constructors

        /// <summary>
        ///     Constructor for root AST block.
        /// </summary>
        internal BlockStatement(Ast parent) {
            Parent     = parent;
            Statements = new NodeList<Statement>(this);
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        internal BlockStatement(SyntaxTreeNode parent, BlockSyntax csMembers) {
            Parent = parent;
            throw new NotImplementedException(csMembers.ToString());
//            Statements = new NodeList<Statement>(
//                this,
//                csMembers.Statements.Select(s => (Statement) CSharpToAxion.ConvertNode(s))
//            );
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        internal BlockStatement(
            SyntaxTreeNode                      parent,
            SyntaxList<MemberDeclarationSyntax> csMembers
        ) {
            Parent = parent;
            throw new NotImplementedException(csMembers.ToString());
//            Statements = new NodeList<Statement>(
//                this,
//                csMembers.Select(m => (Statement) CSharpToAxion.ConvertNode(m))
//            );
        }

        /// <summary>
        ///     Constructs expression from tokens.
        /// </summary>
        internal BlockStatement(SyntaxTreeNode parent, params Statement[] statements) {
            Parent     = parent;
            Statements = new NodeList<Statement>(this, statements);

            if (Statements.Count != 0) {
                MarkPosition(Statements.First, Statements.Last);
            }
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        internal BlockStatement(NodeList<Statement> statements) {
            Statements = statements;

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
                        Parse(parent, type);
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
                        Parse(parent, type);
                    }
                    finally {
                        Ast.InLoop        = wasInLoop;
                        Ast.InFinallyLoop = wasInFinallyLoop;
                    }

                    break;
                }

                case BlockType.Anyway: {
                    bool isInFinally     = Ast.InFinally,
                         isInFinallyLoop = Ast.InFinallyLoop;
                    try {
                        Ast.InFinally     = true;
                        Ast.InFinallyLoop = false;
                        Parse(parent, type);
                    }
                    finally {
                        Ast.InFinally     = isInFinally;
                        Ast.InFinallyLoop = isInFinallyLoop;
                    }

                    break;
                }

                default: {
                    Parse(parent, type);
                    break;
                }
            }
        }

        #endregion

        private void Parse(SyntaxTreeNode parent, BlockType blockType) {
            Parent                             = parent;
            Statements                         = new NodeList<Statement>(this);
            (TokenType terminator, bool error) = ParseStart(this);

            if (terminator == Outdent && blockType == BlockType.Lambda) {
                Unit.Blame(BlameType.LambdaCannotHaveIndentedBody, this);
            }

            if (terminator == Newline) {
                Statements.AddRange(ParseStmt(this));
            }
            else if (!error && !MaybeEat(terminator)) {
                while (true) {
                    Statements.AddRange(ParseStmt(this, terminator));
                    if (MaybeEat(terminator)) {
                        break;
                    }

                    if (Peek.Is(End)) {
                        if (terminator != Outdent) {
                            Unit.Blame(BlameType.UnexpectedEndOfCode, Token);
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
        internal static (TokenType terminator, bool error) ParseStart(SyntaxTreeNode parent) {
            // colon
            bool  hasColon   = parent.MaybeEat(Colon);
            Token blockStart = parent.Token;

            // newline
            bool hasNewline = hasColon
                ? parent.MaybeEatNewline()
                : blockStart.Is(Newline);

            // '{'
            if (parent.MaybeEat(OpenBrace)) {
                if (hasColon) { // ':' '{'
                    parent.Unit.Blame(BlameType.RedundantColonWithBraces, blockStart);
                }

                return (CloseBrace, false);
            }

            // indent
            if (parent.MaybeEat(Indent)) {
                return (Outdent, false);
            }

            if (hasNewline) {
                // newline followed by not indent or '{'
                parent.Unit.Blame(BlameType.ExpectedBlockDeclaration, parent.Peek);
                return (Newline, true);
            }

            // exactly a 1-line block
            if (!hasColon) {
                // one line block must have a colon
                parent.BlameInvalidSyntax(Colon, parent.Peek);
                return (Newline, true);
            }

            return (Newline, false);
        }

        internal bool HasVariable(SimpleNameExpression name) {
            return Variables.Select(v => (v?.Left as SimpleNameExpression)?.Name)
                            .Contains(name?.Name);
        }

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("");
            c.Writer.Indent++;
            c.AddJoin("", Statements, true);
            c.Writer.Indent--;
            c.WriteLine("");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.Writer.Indent++;
            c.AddJoin("", Statements, true);
            c.Writer.Indent--;
            c.Write("}");
        }

        #endregion
    }

    public enum BlockType {
        Default,
        Top,
        Loop,
        Lambda,
        Anyway
    }
}