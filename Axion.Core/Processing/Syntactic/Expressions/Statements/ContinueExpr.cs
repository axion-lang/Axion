using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///         continue-expr:
///             'continue' [name];
///     </code>
/// </summary>
[Branch]
public partial class ContinueExpr : Node {
    [Leaf] Token? kwContinue;
    [Leaf] NameExpr? loopName;

    public ContinueExpr(Node parent) : base(parent) { }

    public ContinueExpr Parse() {
        KwContinue = Stream.Eat(KeywordContinue);
        if (Stream.PeekIs(Identifier)) {
            LoopName = new NameExpr(this).Parse();
        }

        return this;
    }
}
