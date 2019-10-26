using Axion.Core.Processing.CodeGen;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         yield_expr:
    ///             'yield' ('from' infix_expr) | infix_list;
    ///     </c>
    /// </summary>
    public class YieldExpr : Expr, IStatementExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public bool IsYieldFrom { get; set; }

        public YieldExpr(
            Expr parent      = null,
            Expr value       = null,
            bool isYieldFrom = false
        ) : base(parent) {
            Value       = value;
            IsYieldFrom = isYieldFrom;
        }

        public YieldExpr Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordYield);
                if (Stream.MaybeEat(KeywordFrom)) {
                    Value = Parsing.ParseInfix(this);
                }
                else {
                    Value = Parsing.ParseMultiple(this, expectedTypes: typeof(IInfixExpr));
                }
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("yield ");
            if (IsYieldFrom) {
                c.Write("from ");
            }

            c.Write(Value);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("yield return ", Value);
        }

        public override void ToPython(CodeWriter c) {
            c.Write("yield ");
            if (IsYieldFrom) {
                c.Write("from ");
            }

            c.Write(Value);
        }
    }
}