using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         unknown-expr:
    ///             TOKEN* (NEWLINE | END);
    ///     </c>
    /// </summary>
    public class UnknownExpr : AtomExpr {
        public UnknownExpr(Node parent) : base(parent) {
            LangException.Report(BlameType.InvalidSyntax, this);
        }

        public UnknownExpr Parse() {
            SetSpan(
                () => {
                    while (!Stream.PeekIs(Newline, TokenType.End)) {
                        Stream.Eat();
                    }
                }
            );
            return this;
        }
    }
}
