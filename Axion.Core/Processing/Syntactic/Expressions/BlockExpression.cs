using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         block:
    ///               (':' stmt)
    ///             | ([':'] '{' stmt* '}')
    ///             | ([':'] NEWLINE INDENT stmt+ OUTDENT)
    ///     </c>
    /// </summary>
    public class BlockExpression : Expression {
        private NodeList<Expression> items;

        public NodeList<Expression> Items {
            get => items;
            set => SetNode(ref items, value);
        }

        public void RegisterNamedNode(AstNode node, Action parser = null) {
            switch (node) {
            case ModuleDefinition m: {
                modules.Add(m);
                CurrentModule = m;
                parser?.Invoke();
                CurrentModule = null;
                break;
            }

            case IFunctionNode f: {
                functions.Add(f);
                CurrentFunction = f;
                parser?.Invoke();
                CurrentFunction = null;
                break;
            }

            case ClassDefinition c: {
                classes.Add(c);
                CurrentClass = c;
                parser?.Invoke();
                CurrentClass = null;
                break;
            }

            case EnumDefinition e: {
                enums.Add(e);
                CurrentEnum = e;
                parser?.Invoke();
                CurrentEnum = null;
                break;
            }

            case VariableDefinitionExpression v: {
                variables.Add(v);
                break;
            }

            default: {
                throw new NotSupportedException();
            }
            }
        }

        private readonly List<VariableDefinitionExpression> variables =
            new List<VariableDefinitionExpression>();

        internal bool HasVariable(SimpleNameExpression name) {
            return variables.Select(v => (v.Left as SimpleNameExpression)?.Name)
                            .Contains(name.Name);
        }

        private readonly List<IFunctionNode> functions = new List<IFunctionNode>();
        public           IFunctionNode       CurrentFunction { get; private set; }

        public void PopFunction() {
            CurrentFunction = null;
        }

        private readonly List<ClassDefinition> classes = new List<ClassDefinition>();
        public           ClassDefinition       CurrentClass { get; private set; }

        public void PopClass() {
            CurrentClass = null;
        }

        private readonly List<EnumDefinition> enums = new List<EnumDefinition>();
        public           EnumDefinition       CurrentEnum { get; private set; }

        public void PopEnum() {
            CurrentEnum = null;
        }

        private readonly List<ModuleDefinition> modules = new List<ModuleDefinition>();
        public           ModuleDefinition       CurrentModule { get; private set; }

        public void PopModule() {
            CurrentModule = null;
        }

        #region Constructors

        /// <summary>
        ///     Constructs from C# syntax.
        /// </summary>
        internal BlockExpression(AstNode parent, BlockSyntax csMembers) : base(parent) {
            throw new NotImplementedException(csMembers.ToString());
//            Items = new NodeList<Expression>(
//                this,
//                csMembers.Statements.Select(s => (Expression) CSharpToAxion.ConvertNode(s))
//            );
        }

        /// <summary>
        ///     Constructs from C# syntax.
        /// </summary>
        internal BlockExpression(
            AstNode                             parent,
            SyntaxList<MemberDeclarationSyntax> csMembers
        ) : base(parent) {
            throw new NotImplementedException(csMembers.ToString());
//            Items = new NodeList<Expression>(
//                this,
//                csMembers.Select(m => (Expression) CSharpToAxion.ConvertNode(m))
//            );
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal BlockExpression(AstNode parent, params Expression[] statements) :
            base(parent) {
            Items = new NodeList<Expression>(this, statements);

            if (Items.Count != 0) {
                MarkPosition(Items.First, Items.Last);
            }
        }

        /// <summary>
        ///     Constructs without position in source.
        /// </summary>
        internal BlockExpression(NodeList<Expression> statements) {
            Items = statements;

            if (Items.Count != 0) {
                MarkPosition(Items.First, Items.Last);
            }
        }

        internal BlockExpression(AstNode parent, BlockType type = BlockType.Default) :
            base(parent) {
            switch (type) {
            case BlockType.Named: {
                RegisterNamedNode(
                    parent,
                    () => {
                        bool isInLoop        = Ast.InLoop,
                             isInFinally     = Ast.InFinally,
                             isInFinallyLoop = Ast.InFinallyLoop;
                        try {
                            Ast.InLoop        = false;
                            Ast.InFinally     = false;
                            Ast.InFinallyLoop = false;
                            Parse(type);
                        }
                        finally {
                            Ast.InLoop        = isInLoop;
                            Ast.InFinally     = isInFinally;
                            Ast.InFinallyLoop = isInFinallyLoop;
                        }

                        PopFunction();
                    }
                );


                break;
            }

            case BlockType.Loop: {
                bool wasInLoop        = Ast.InLoop,
                     wasInFinallyLoop = Ast.InFinallyLoop;
                try {
                    Ast.InLoop        = true;
                    Ast.InFinallyLoop = Ast.InFinally;
                    Parse(type);
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
                    Parse(type);
                }
                finally {
                    Ast.InFinally     = isInFinally;
                    Ast.InFinallyLoop = isInFinallyLoop;
                }

                break;
            }

            default: {
                Parse(type);
                break;
            }
            }
        }

        protected BlockExpression() { }

        #endregion

        private void Parse(BlockType blockType) {
            Items                              = new NodeList<Expression>(this);
            (TokenType terminator, bool error) = ParseStart(this);

            if (terminator == Outdent && blockType.HasFlag(BlockType.Lambda)) {
                Unit.Blame(BlameType.LambdaCannotHaveIndentedBody, this);
            }

            if (terminator == Newline) {
                Items.AddRange(ParseCascade(this));
            }
            else if (!error && !MaybeEat(terminator)) {
                while (true) {
                    Items.AddRange(ParseCascade(this, terminator));
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
        internal static (TokenType terminator, bool error) ParseStart(AstNode parent) {
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

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("");
            c.Writer.Indent++;
            c.AddJoin("", Items, true);
            c.Writer.Indent--;
            c.WriteLine("");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.Writer.Indent++;
            c.AddJoin("", Items, true);
            c.Writer.Indent--;
            c.Write("}");
        }

        #endregion
    }

    [Flags]
    public enum BlockType {
        Default,
        Named,
        Loop,
        Lambda,
        Anyway
    }
}