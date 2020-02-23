using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         conditional_expr:
    ///             'if' infix_expr block
    ///             {'elif' infix_expr block}
    ///             ['else' block];
    ///     </c>
    /// </summary>
    public class ConditionalExpr : Expr, IDecoratedExpr {
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private BlockExpr thenBlock;

        public BlockExpr ThenBlock {
            get => thenBlock;
            set => SetNode(ref thenBlock, value);
        }

        private BlockExpr elseBlock;

        public BlockExpr ElseBlock {
            get => elseBlock;
            set => SetNode(ref elseBlock, value);
        }

        internal ConditionalExpr(
            Expr      parent    = null,
            Expr      condition = null,
            BlockExpr thenBlock = null,
            BlockExpr elseBlock = null
        ) : base(parent) {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }

        public ConditionalExpr Parse(bool elseIf = false) {
            SetSpan(
                () => {
                    if (!elseIf) {
                        Stream.Eat(KeywordIf);
                    }

                    Condition = InfixExpr.Parse(this);
                    ThenBlock = new BlockExpr(this).Parse();

                    if (Stream.MaybeEat(KeywordElse)) {
                        ElseBlock = new BlockExpr(this).Parse();
                    }
                    else if (Stream.MaybeEat(KeywordElif)) {
                        ElseBlock = new BlockExpr(this, new ConditionalExpr(this).Parse(true));
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("if ", Condition, ThenBlock);
            if (ElseBlock != null) {
                c.Write("else", ElseBlock);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("if (", Condition);
            c.WriteLine(")");
            c.Write(ThenBlock);
            if (ElseBlock != null) {
                c.WriteLine("else");
                c.Write(ElseBlock);
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write("if ", Condition, ThenBlock);
            if (ElseBlock != null) {
                c.Write("else", ElseBlock);
            }
        }

        public override void ToPascal(CodeWriter c) {
            c.Write("if ", Condition, " then", ThenBlock);
            if (ElseBlock != null) {
                c.Write("else", ElseBlock);
            }
        }
    }
}