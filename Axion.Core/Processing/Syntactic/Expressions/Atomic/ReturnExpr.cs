using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         return_expr:
    ///             'return' [expr_list];
    ///     </c>
    /// </summary>
    public class ReturnExpr : Expr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        public ReturnExpr(
            Expr parent = null,
            Expr value  = null
        ) : base(parent) {
            Value = value;
        }

        public ReturnExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordReturn);
                    if (!Stream.PeekIs(Spec.NeverExprStartTypes)) {
                        Value = Parsing.MultipleExprs(this);
                    }
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("return");
            if (Value != null) {
                c.Write(" ", Value);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("return");
            if (Value != null) {
                c.Write(" ", Value);
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write("return");
            if (Value != null) {
                c.Write(" ", Value);
            }
        }
    }
}