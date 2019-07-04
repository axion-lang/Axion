using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Binary;
using Axion.Core.Processing.Syntactic.Definitions;
using Axion.Core.Processing.Syntactic.Interfaces;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic {
    /// <summary>
    ///     <c>
    ///         block:
    ///             (':' stmt)
    ///             | ([':'] '{' stmt* '}')
    ///             | ([':'] NEWLINE INDENT stmt+ OUTDENT)
    ///     </c>
    /// </summary>
    public class BlockExpression : Expression {
        private NodeList<Expression> items;

        public NodeList<Expression> Items {
            get => items;
            protected set => SetNode(ref items, value);
        }

        internal bool InLoop;

        #region Registered named nodes

        private readonly List<VariableDefinitionExpression> variables =
            new List<VariableDefinitionExpression>();

        internal bool HasVariable(SimpleNameExpression name) {
            return variables.Select(v => (v.Left as SimpleNameExpression)?.Name)
                            .Contains(name.Name);
        }

        private readonly List<IFunctionNode> functions = new List<IFunctionNode>();
        public IFunctionNode CurrentFunction { get; private set; }

        public void PopFunction() {
            CurrentFunction = null;
        }

        private readonly List<ClassDefinition> classes = new List<ClassDefinition>();
        public ClassDefinition CurrentClass { get; private set; }

        public void PopClass() {
            CurrentClass = null;
        }

        private readonly List<EnumDefinition> enums = new List<EnumDefinition>();
        public EnumDefinition CurrentEnum { get; private set; }

        public void PopEnum() {
            CurrentEnum = null;
        }

        private readonly List<ModuleDefinition> modules = new List<ModuleDefinition>();
        public ModuleDefinition CurrentModule { get; private set; }

        public void PopModule() {
            CurrentEnum = null;
        }

        public void RegisterNamedNode(Expression node, Action parser = null) {
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

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs from C# syntax.
        /// </summary>
        internal BlockExpression(Expression parent, BlockSyntax csMembers) : base(parent) {
            throw new NotImplementedException(csMembers.ToString());
            // Items = new NodeList<Expression>(
            //     this,
            //     csMembers.Statements.Select(s => (Expression) CSharpToAxion.ConvertNode(s))
            // );
        }

        /// <summary>
        ///     Constructs from C# syntax.
        /// </summary>
        internal BlockExpression(
            Expression                          parent,
            SyntaxList<MemberDeclarationSyntax> csMembers
        ) : base(parent) {
            throw new NotImplementedException(csMembers.ToString());
            // Items = new NodeList<Expression>(
            //     this,
            //     csMembers.Select(m => (Expression) CSharpToAxion.ConvertNode(m))
            // );
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal BlockExpression(Expression parent, params Expression[] statements) :
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

        internal BlockExpression(Expression parent, BlockType type = BlockType.Default) :
            base(parent) {
            switch (type) {
            case BlockType.Named: {
                RegisterNamedNode(
                    parent,
                    () => {
                        bool isInLoop = InLoop;
                        InLoop = false;
                        Parse(type);
                        InLoop = isInLoop;

                        PopFunction();
                    }
                );


                break;
            }

            case BlockType.Loop: {
                bool wasInLoop = InLoop;

                InLoop = true;
                Parse(type);
                InLoop = wasInLoop;
                break;
            }

            default: {
                InLoop = ParentBlock.InLoop;
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
        internal static (TokenType terminator, bool error) ParseStart(Expression parent) {
            // colon
            bool  hasColon   = parent.MaybeEat(Colon);
            Token blockStart = parent.Token;

            // newline
            bool hasNewline = hasColon
                ? parent.MaybeEat(Newline)
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

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("");
            if (this is Ast) {
                c.AddJoin("", Items, true);
            }
            else {
                c.Writer.Indent++;
                c.AddJoin("", Items, true);
                c.Writer.Indent--;
            }

            c.WriteLine("");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.Writer.Indent++;
            c.AddJoin(";", Items, true);
            c.WriteLine(";");
            c.Writer.Indent--;
            c.Write("}");
        }
    }

    [Flags]
    public enum BlockType {
        Default,
        Named,
        Loop,
        Lambda
    }
}