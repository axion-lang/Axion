using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         unknown_expr:
    ///             TOKEN* (NEWLINE | END);
    ///     </c>
    /// </summary>
    public class UnknownExpr : Expr {
        public UnknownExpr(Expr parent) : base(parent) {
            LangException.Report(BlameType.InvalidSyntax, this);
        }

        public UnknownExpr Parse() {
            SetSpan(() => {
                while (!Stream.PeekIs(Newline, TokenType.End)) {
                    Stream.Eat();
                }
            });
            return this;
        }
    }
}