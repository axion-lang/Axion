using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Interfaces;
using Axion.Core.Specification;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def:
    ///             'module' name block
    ///     </c>
    /// </summary>
    public class ModuleDefinition : Expression, IDecorable {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockExpression block;

        public BlockExpression Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expression> modifiers;

        public NodeList<Expression> Modifiers {
            get => modifiers;
            set => SetNode(ref modifiers, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        internal ModuleDefinition(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.KeywordModule);

            Name  = NameExpression.ParseName(parent);
            Block = new BlockExpression(this, BlockType.Named);

            MarkEnd();
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        internal ModuleDefinition(NamespaceDeclarationSyntax csNode) {
            Name  = NameExpression.ParseName(this, csNode.Name.ToString());
            Block = new BlockExpression(this, csNode.Members);
        }

        /// <summary>
        ///     Constructs expression without position in source.
        /// </summary>
        internal ModuleDefinition(
            AstNode         parent,
            NameExpression  name,
            BlockExpression block
        ) : base(parent) {
            Name  = name;
            Block = block;
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("module ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("namespace ", Name, Block);
        }
    }
}