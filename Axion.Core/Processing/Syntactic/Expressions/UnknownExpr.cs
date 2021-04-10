using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.SourceGenerators;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <code>
    ///         unknown-expr:
    ///             TOKEN* (NEWLINE | END);
    ///     </code>
    /// </summary>
    [SyntaxExpression]
    public partial class UnknownExpr : AtomExpr {
        [LeafSyntaxNode] NodeList<Token>? tokens;

        public UnknownExpr(Node parent) : base(parent) {
            LanguageReport.To(BlameType.InvalidSyntax, this);
        }

        public UnknownExpr Parse() {
            while (!Stream.PeekIs(Newline, TokenType.End)) {
                Tokens += Stream.Eat();
            }

            return this;
        }
    }
}
