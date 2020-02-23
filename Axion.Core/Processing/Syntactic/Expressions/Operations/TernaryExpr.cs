using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary_expr:
    ///             expr_list ('if' | 'unless') infix_expr ['else' expr_list];
    ///     </c>
    /// </summary>
    public class TernaryExpr : Expr {
        private Expr condition;

        public Expr Condition {
            get => condition;
            set => SetNode(ref condition, value);
        }

        private Expr trueExpr;

        public Expr TrueExpr {
            get => trueExpr;
            set => SetNode(ref trueExpr, value);
        }

        private Expr falseExpr;

        public Expr FalseExpr {
            get => falseExpr;
            set => SetNode(ref falseExpr, value);
        }

        [NoTraversePath]
        public override TypeName ValueType => TrueExpr.ValueType;

        internal TernaryExpr(
            Expr parent    = null,
            Expr condition = null,
            Expr trueExpr  = null,
            Expr falseExpr = null
        ) : base(parent) {
            Condition = condition;
            TrueExpr  = trueExpr;
            FalseExpr = falseExpr;
        }

        public TernaryExpr Parse() {
            SetSpan(
                () => {
                    var invert = false;
                    if (!Stream.MaybeEat(KeywordIf)) {
                        Stream.Eat(KeywordUnless);
                        invert = true;
                    }

                    if (TrueExpr == null) {
                        TrueExpr = AnyExpr.Parse(this);
                    }

                    Condition = InfixExpr.Parse(this);
                    if (Stream.MaybeEat(KeywordElse)) {
                        FalseExpr = Parsing.MultipleExprs(this, expectedTypes: typeof(IInfixExpr));
                    }

                    if (invert) {
                        (TrueExpr, FalseExpr) = (FalseExpr, TrueExpr);
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(TrueExpr, " if ", Condition);
            if (FalseExpr != null) {
                c.Write(" else ", FalseExpr);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(Condition, " ? ", TrueExpr, " : ");
            if (FalseExpr == null) {
                c.Write("default");
            }
            else {
                c.Write(FalseExpr);
            }
        }

        public override void ToPython(CodeWriter c) {
            ToAxion(c);
        }
    }
}