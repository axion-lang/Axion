using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///         break-expr:
///             'break' [name];
///     </code>
/// </summary>
[Branch]
public partial class BreakExpr : Node {
    [Leaf] Token? kwBreak;
    [Leaf] NameExpr? loopName;

    public BreakExpr(Node parent) : base(parent) { }

    public BreakExpr Parse() {
        KwBreak = Stream.Eat(KeywordBreak);
        if (Stream.PeekIs(Identifier)) {
            LoopName = new NameExpr(this).Parse();
        }

        return this;
    }
}
