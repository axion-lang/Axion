using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         code-unquoted-expr:
    ///             '$' expr;
    ///     </c>
    /// </summary>
    public class CodeUnquotedExpr : PostfixExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => val = Bind(value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        public CodeUnquotedExpr(
            Expr? parent = null,
            Expr? value  = null
        ) : base(
            parent
         ?? GetParentFromChildren(value)
        ) {
            Value = value;
        }

        public CodeUnquotedExpr Parse() {
            SetSpan(
                () => {
                    Stream.Eat(TokenType.Dollar);
                    Value = Parse(this);
                }
            );
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("$", Value);
        }
    }
}