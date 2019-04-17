using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def:
    ///             'module' name block
    ///     </c>
    /// </summary>
    public class ModuleDefinition : Statement, IDecorated {
        #region Properties

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

        #endregion

        #region Constructors

        /// <summary>
        ///     Constructs new <see cref="ModuleDefinition"/> from Axion tokens.
        /// </summary>
        internal ModuleDefinition(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordModule);

            Name  = new NameExpression(this);
            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
        }

        /// <summary>
        ///     Constructs new <see cref="ModuleDefinition"/> from C# syntax.
        /// </summary>
        internal ModuleDefinition(NamespaceDeclarationSyntax csNode) {
            Name  = new NameExpression(this, csNode.Name);
            Block = new BlockStatement(this, csNode.Members);
        }

        /// <summary>
        ///     Constructs plain <see cref="ModuleDefinition"/> without position in source.
        /// </summary>
        internal ModuleDefinition(NameExpression name, BlockStatement block) {
            Name  = name;
            Block = block;

            MarkPosition(Name, Block);
        }

        #endregion

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("module ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("namespace ", Name, Block);
        }

        #endregion
    }
}