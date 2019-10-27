using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Traversal;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    /// <summary>
    ///     <c>
    ///         code_unquoted_expr:
    ///             '$' expr;
    ///     </c>
    /// </summary>
    public class CodeUnquotedExpr : Expr, IAtomExpr {
        private Expr val;

        public Expr Value {
            get => val;
            set => SetNode(ref val, value);
        }

        [NoTraversePath]
        public override TypeName ValueType => Value.ValueType;

        public CodeUnquotedExpr(
            Expr parent = null,
            Expr value  = null
        ) : base(parent) {
            Value = value;
        }

        public CodeUnquotedExpr Parse() {
            SetSpan(() => {
                Stream.Eat(TokenType.Dollar);
                Value = Parsing.ParsePostfix(this);
            });
            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write("$", Value);
        }
    }
}