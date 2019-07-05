using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def:
    ///             'module' name block;
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ModuleDefinition(Expression parent) {
            Construct(parent, () => {
                Eat(KeywordModule);

                Name  = NameExpression.ParseName(parent);
                Block = new BlockExpression(this, BlockType.Named);
            });
        }

        /// <summary>
        ///     Constructs expression from C# syntax.
        /// </summary>
        internal ModuleDefinition(NamespaceDeclarationSyntax csNode) {
            Name  = NameExpression.ParseName(this, csNode.Name.ToString());
            Block = new BlockExpression(this, csNode.Members);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("module ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("namespace ", Name, Block);
        }
    }
}