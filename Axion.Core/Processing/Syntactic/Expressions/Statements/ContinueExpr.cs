using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         continue-expr:
    ///             'continue' [name];
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class ContinueExpr : Node {
        [LeafSyntaxNode] Token? kwContinue;
        [LeafSyntaxNode] NameExpr? loopName;

        public ContinueExpr(Node parent) : base(parent) { }

        public ContinueExpr Parse() {
            KwContinue = Stream.Eat(KeywordContinue);
            if (Stream.PeekIs(Identifier)) {
                LoopName = new NameExpr(this).Parse();
            }

            return this;
        }
    }
}
