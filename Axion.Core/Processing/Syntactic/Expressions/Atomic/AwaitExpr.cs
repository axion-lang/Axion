using Axion.Core.Processing.CodeGen;
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
        private Expr val;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        public AwaitExpr(
            Expr? parent = null,
            Expr? value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Value = value;
        }

        public AwaitExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(KeywordAwait);
                    Value = Multiple<Expr>.ParseGenerally(this);
                }
            );
            return this;
        }

        public override void ToDefault(CodeWriter c) {
            c.Write("await ", Value);
        }
    }
}