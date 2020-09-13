using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
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
        private Token? kwReturn;

        public Token? KwReturn {
            get => kwReturn;
            set => kwReturn = BindNullable(value);
        }

        private Expr? val;

        public Expr? Value {
            get => val;
            set => val = BindNullable(value);
        }

        public override TypeName? ValueType => Value?.ValueType;

        public ReturnExpr(Node parent) : base(parent) { }

        public ReturnExpr Parse() {
            KwReturn = Stream.Eat(KeywordReturn);
            if (!Stream.PeekIs(Spec.NeverExprStartTypes)) {
                Value = Multiple<InfixExpr>.ParseGenerally(this);
            }
            return this;
        }
    }
}
