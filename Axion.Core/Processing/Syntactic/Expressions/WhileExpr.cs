using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         while_expr:
    ///             'while' infix block
    ///             ['nobreak' block];
    ///     </c>
    /// </summary>
    public class WhileExpr : Expr, IStatementExpr, IDecoratedExpr {
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockExpr block;

        public BlockExpr Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private BlockExpr noBreakBlock;

        public BlockExpr NoBreakBlock {
            get => noBreakBlock;
            set => SetNode(ref noBreakBlock, value);
        }

        public WhileExpr(
            Expr      parent,
            Expr      condition    = null,
            BlockExpr block        = null,
            BlockExpr noBreakBlock = null
        ) : base(parent) {
            Condition    = condition;
            Block        = block;
            NoBreakBlock = noBreakBlock;
        }

        public Expr Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordWhile);
                Condition = InfixExpr.Parse(this);
                Block     = new BlockExpr(this).Parse();
                if (Stream.MaybeEat(KeywordNoBreak)) {
                    NoBreakBlock = new BlockExpr(this).Parse();
                }
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("while ", Condition, Block);
            if (NoBreakBlock != null) {
                c.Write("nobreak", NoBreakBlock);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("while (", Condition);
            c.WriteLine(")");
            c.Write(Block);
        }

        public override void ToPython(CodeWriter c) {
            c.Write("while ", Condition, Block);
        }

        public override void ToPascal(CodeWriter c) {
            c.Write("while ", Condition, " do", Block);
        }
    }
}