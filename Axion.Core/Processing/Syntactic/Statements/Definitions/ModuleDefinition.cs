using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def:
    ///             'module' name block
    ///     </c>
    /// </summary>
    public class ModuleDefinition : Statement, IDecorated {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expression> modifiers;

        public NodeList<Expression> Modifiers {
            get => modifiers ??= new NodeList<Expression>(this);
            set {
                if (value != null) {
                    modifiers = value;
                }
            }
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        internal ModuleDefinition(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordModule);

            Name  = NameExpression.ParseName(parent);
            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
            ParentBlock.Modules.Add(this);
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        internal ModuleDefinition(NamespaceDeclarationSyntax csNode) {
            Name  = NameExpression.ParseName(this, csNode.Name.ToString());
            Block = new BlockStatement(this, csNode.Members);
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        internal ModuleDefinition(
            SyntaxTreeNode parent,
            NameExpression name,
            BlockStatement block
        ) : base(parent) {
            Name  = name;
            Block = block;

            MarkPosition(Name, Block);
            ParentBlock.Modules.Add(this);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("module ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("namespace ", Name, Block);
        }
    }
}