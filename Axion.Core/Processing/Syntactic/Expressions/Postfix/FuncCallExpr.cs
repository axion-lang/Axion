using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix {
    /// <summary>
    ///     <c>
    ///         func-call-expr:
    ///             atom '(' [multiple-arg | (arg for-comprehension)] ')';
    ///     </c>
    /// </summary>
    public class FuncCallExpr : PostfixExpr {
        private Expr target;

        public Expr Target {
            get => target;
            set => target = Bind(value);
        }

        private NodeList<FuncCallArg> args;

        public NodeList<FuncCallArg> Args {
            get => args;
            set => args = Bind(value);
        }

        public FuncCallExpr(Expr? parent = null, Expr? target = null, params FuncCallArg[] args) :
            base(parent ?? GetParentFromChildren(target)) {
            Target = target;
            Args   = NodeList<FuncCallArg>.From(this, args);
        }

        public FuncCallExpr Parse(bool allowGenerator = false) {
            SetSpan(
                () => {
                    Stream.Eat(OpenParenthesis);
                    Args = FuncCallArg.ParseArgList(this, allowGenerator: allowGenerator);
                    Stream.Eat(CloseParenthesis);
                }
            );
            return this;
        }
    }
}
