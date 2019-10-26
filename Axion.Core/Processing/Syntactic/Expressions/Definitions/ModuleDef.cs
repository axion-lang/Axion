using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         module_def:
    ///             'module' name block;
    ///     </c>
    /// </summary>
    public class ModuleDef : Expr, IDefinitionExpr, IDecoratedExpr {
        private Expr name;

        public Expr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expr> modifiers;

        public NodeList<Expr> Modifiers {
            get => modifiers;
            set => SetNode(ref modifiers, value);
        }

        internal ModuleDef(
            Expr           parent    = null,
            NameExpr       name      = null,
            BlockExpr      block     = null,
            NodeList<Expr> modifiers = null
        ) : base(parent) {
            Name      = name;
            Block     = block;
            Modifiers = modifiers;
        }

        public ModuleDef Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordModule);
                Name  = new NameExpr(this).Parse();
                Block = new BlockExpr(this).Parse();
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("module ", Name, Block);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("namespace ", Name, Block);
        }

        public override void ToPython(CodeWriter c) {
            c.AddJoin("", Block.Items, true);
        }
    }
}