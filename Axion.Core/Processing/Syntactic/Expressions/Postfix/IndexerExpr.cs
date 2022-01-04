using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Attributes;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Postfix;

/// <summary>
///     <code>
///         index-expr:
///             atom '[' (infix-expr | slice) {',' (infix-expr | slice)} [','] ']';
///         slice:
///             [infix-expr] ':' [infix-expr] [':' [infix-expr]];
///     </code>
/// </summary>
[Branch]
public partial class IndexerExpr : PostfixExpr {
    [Leaf] Node? index;
    [Leaf] Node? target;

    public IndexerExpr(Node parent) : base(parent) { }

    public IndexerExpr Parse() {
        Target ??= AtomExpr.Parse(this);
        var expressions = new NodeList<Node, Ast>(this);
        Stream.Eat(OpenBracket);
        if (!Stream.PeekIs(CloseBracket)) {
            while (true) {
                Node? start = null;
                if (!Stream.PeekIs(Colon)) {
                    start = InfixExpr.Parse(this);
                }

                if (Stream.MaybeEat(Colon)) {
                    Node? stop = null;
                    if (!Stream.PeekIs(
                            Colon,
                            Comma,
                            CloseBracket
                        )) {
                        stop = InfixExpr.Parse(this);
                    }

                    Node? step = null;
                    if (Stream.MaybeEat(Colon)
                     && !Stream.PeekIs(Comma, CloseBracket)) {
                        step = InfixExpr.Parse(this);
                    }

                    expressions += new SliceExpr(this) {
                        From = start,
                        To   = stop,
                        Step = step
                    };
                    break;
                }

                if (start == null) {
                    LanguageReport.To(
                        BlameType.InvalidIndexerExpression,
                        Stream.Token
                    );
                }
                else {
                    expressions += start;
                }

                if (Stream.PeekIs(CloseBracket)) {
                    break;
                }

                Stream.Eat(Comma);
            }
        }

        Index = expressions.Count == 1
            ? expressions[0]
            : new TupleExpr(this) {
                Expressions = expressions
            };
        End = Stream.Eat(CloseBracket).End;
        return this;
    }
}
