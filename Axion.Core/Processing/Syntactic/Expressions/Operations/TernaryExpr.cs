using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Operations {
    /// <summary>
    ///     <c>
    ///         ternary-expr:
    ///             multiple-expr ('if' | 'unless') infix-expr ['else' multiple-expr];
    ///     </c>
    /// </summary>
    public class TernaryExpr : InfixExpr {
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
            Expr? parent    = null,
            Expr? condition = null,
            Expr? trueExpr  = null,
            Expr? falseExpr = null
        ) : base(
            parent
         ?? GetParentFromChildren(condition, trueExpr, falseExpr)
        ) {
            Condition = condition;
            TrueExpr  = trueExpr;
            FalseExpr = falseExpr;
            MarkStart(TrueExpr);
            MarkEnd(FalseExpr);
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

                    Condition = Parse(this);
                    if (Stream.MaybeEat(KeywordElse)) {
                        FalseExpr = Multiple<InfixExpr>.ParseGenerally(this);
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