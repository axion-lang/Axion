using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Generic;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         await-expr:
    ///             'await' multiple-expr;
    ///     </c>
    /// </summary>
    public class AwaitExpr : AtomExpr {
        private Token? kwAwait;

        public Token? KwAwait {
            get => kwAwait;
            set => kwAwait = BindNullable(value);
        }

        private Expr val = null!;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        public override TypeName ValueType => Value.ValueType;

        public AwaitExpr(Node parent) : base(parent) { }

        public AwaitExpr Parse() {
            KwAwait = Stream.Eat(KeywordAwait);
            Value   = Multiple<Expr>.ParseGenerally(this);
            return this;
        }
    }
}
