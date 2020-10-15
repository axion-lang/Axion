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
        private NodeList<Token>? tokens;

        public NodeList<Token> Tokens {
            get => InitIfNull(ref tokens);
            set => tokens = Bind(value);
        }

        public UnknownExpr(Node parent) : base(parent) {
            LangException.Report(BlameType.InvalidSyntax, this);
        }

        public UnknownExpr Parse() {
            while (!Stream.PeekIs(Newline, TokenType.End)) {
                Tokens += Stream.Eat();
            }

            return this;
        }
    }
}
