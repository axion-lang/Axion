using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         yield-expr:
    ///             'yield' ('from' infix-expr) | multiple-infix;
    ///     </c>
    /// </summary>
    public class YieldExpr : AtomExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => val = BindNode(value);
        }

        public bool IsYieldFrom { get; set; }

        public YieldExpr(
            Expr? parent      = null,
            Expr? value       = null,
            bool  isYieldFrom = false
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Value       = value;
            IsYieldFrom = isYieldFrom;
        }

        public YieldExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordYield);
                    if (Stream.MaybeEat("from")) {
                        Value = InfixExpr.Parse(this);
                    }
                    else {
                        Value = Multiple<InfixExpr>.ParseGenerally(this);
                    }
                }
            );
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