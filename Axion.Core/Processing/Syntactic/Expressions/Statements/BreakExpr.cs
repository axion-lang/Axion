using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.SourceGenerators;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements {
    /// <summary>
    ///     <c>
    ///         break-expr:
    ///             'break' [name];
    ///     </c>
    /// </summary>
    [SyntaxExpression]
    public partial class BreakExpr : Node {
        [LeafSyntaxNode] Token? kwBreak;
        [LeafSyntaxNode] NameExpr? loopName;

        public BreakExpr(Node parent) : base(parent) { }

        public BreakExpr Parse() {
            KwBreak = Stream.Eat(KeywordBreak);
            if (Stream.PeekIs(Identifier)) {
                LoopName = new NameExpr(this).Parse();
            }

            return this;
        }
    }
}
