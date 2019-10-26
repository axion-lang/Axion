using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         await_expr:
    ///             'await' expr_list;
    ///     </c>
    /// </summary>
    public class AwaitExpr : Expr, IAtomExpr, IStatementExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        public AwaitExpr(
            Expr parent = null,
            Expr value  = null
        ) : base(parent) {
            Value = value;
        }

        public AwaitExpr Parse() {
            SetSpan(() => {
                Stream.Eat(KeywordAwait);
                Value = Parsing.ParseMultiple(this);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("await ", Value);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("await ", Value);
        }

        public override void ToPython(CodeWriter c) {
            c.Write("await ", Value);
        }
    }
}