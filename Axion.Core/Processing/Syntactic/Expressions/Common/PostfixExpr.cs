using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Specification;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common;

/// <summary>
///     "Postfix" expression is any <see cref="AtomExpr" />
///     that may be followed by non-infix (terminal) token
///     and maybe other next parts.
///     <code>
///         postfix:
///             atom
///             { member
///             | call-expr
///             | index-expr }
///             ['++' | '--'];
///     </code>
/// </summary>
public class PostfixExpr : PrefixExpr {
    protected PostfixExpr(Node? parent) : base(parent) { }

    internal new static PostfixExpr Parse(Node parent) {
        var s = parent.Unit.TokenStream;

        // TODO: look about it.
        var isUnquoted = !s.PeekByIs(2, OpenParenthesis) && s.MaybeEat(Dollar);
        PostfixExpr value = AtomExpr.Parse(parent);

        while (true) {
            var exactPeek = s.ExactPeek;
            if (s.PeekIs(Dot)) {
                value = new MemberAccessExpr(parent) {
                    Target = value
                }.Parse();
            }
            // NOTE: This condition makes _impossible_ placement of
            //       function invocation parenthesis on the next line, e.g:
            //           x = func
            //           ()
            //       But this also resolves conflicts when stmt
            //       starting with open paren is incorrectly treated
            //       as continuation of previous stmt, e.g:
            //           x = not-a-function-just-a-value
            //           (a, b) = (1, 2)
            else if (s.PeekIs(OpenParenthesis) && s.Peek == exactPeek) {
                value = new FuncCallExpr(parent) {
                    Target = value
                }.Parse(true);
            }
            else if (s.PeekIs(OpenBracket)) {
                value = new IndexerExpr(parent) {
                    Target = value
                }.Parse();
            }
            else {
                break;
            }
        }

        if (s.MaybeEat(DoublePlus, DoubleMinus)) {
            var op = (OperatorToken) s.Token;
            op.Side = InputSide.Left;
            value = new UnaryExpr(parent) {
                Operator = op,
                Value    = value
            };
        }

        if (isUnquoted) {
            value = new CodeUnquotedExpr(parent) {
                Value = value
            };
        }

        return value;
    }
}
