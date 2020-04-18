using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         await-expr:
    ///             'await' multiple-expr;
    ///     </c>
    /// </summary>
    public class AwaitExpr : AtomExpr {
        private Expr val = null!;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        [NoPathTraversing]
        public override TypeName ValueType => Value.ValueType;

        public AwaitExpr(Expr parent) : base(parent) { }

        public AwaitExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordAwait);
                    Value = Multiple<Expr>.ParseGenerally(this);
                }
            );
            return this;
        }
    }
}
