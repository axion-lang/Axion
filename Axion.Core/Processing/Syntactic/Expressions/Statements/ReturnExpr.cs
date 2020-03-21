using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         return-expr:
    ///             'return' [multiple-expr];
    ///     </c>
    /// </summary>
    public class ReturnExpr : Expr {
        private Expr val;

        public Expr Value {
            get => val;
            set => val = BindNode(value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        public ReturnExpr(
            Expr? parent = null,
            Expr? value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Value = value;
        }

        public ReturnExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordReturn);
                    if (!Stream.PeekIs(Spec.NeverExprStartTypes)) {
                        Value = Multiple<InfixExpr>.ParseGenerally(this);
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