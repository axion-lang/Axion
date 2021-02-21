using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         code-unquoted-expr:
    ///             '$' expr;
    ///     </c>
    /// </summary>
    public class CodeUnquotedExpr : PostfixExpr {
        private Token? startMark;

        public Token? StartMark {
            get => startMark;
            set => startMark = BindNullable(value);
        }

        private Expr val = null!;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        public override TypeName? ValueType => Value.ValueType;

        public CodeUnquotedExpr(Node parent) : base(parent) { }

        public CodeUnquotedExpr Parse() {
            StartMark = Stream.Eat(TokenType.Dollar);
            Value     = Parse(this);
            return this;
        }
    }
}
