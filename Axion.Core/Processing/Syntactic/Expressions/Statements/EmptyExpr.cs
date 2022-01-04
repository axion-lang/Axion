using Axion.Core.Processing.Lexical.Tokens;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Statements;

/// <summary>
///     <code>
///         empty-expr:
///             ';' | 'pass';
///     </code>
/// </summary>
[Branch]
public partial class EmptyExpr : Node {
    [Leaf] Token? mark;

    public EmptyExpr(Node parent) : base(parent) { }

    public EmptyExpr Parse() {
        Mark = Stream.Eat(Semicolon, KeywordPass);
        return this;
    }
}
