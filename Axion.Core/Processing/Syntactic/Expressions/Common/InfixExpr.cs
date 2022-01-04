using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using Axion.Specification;
using Magnolia.Attributes;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common;

/// <summary>
///     [Strictness appears here.]
///     "Infix" is an expression that usually consists
///     of 3 parts.
///     <br />
///     Infix includes
///     <br />
///     - Binary/ternary operations;
///     <br />
///     - Infix-macros (that start with an expression, then token);
///     <br />
///     - And also any <see cref="PrefixExpr" />.
///     <code>
///         infix
///             : prefix (ID | SYMBOL) infix
///             | for-comprehension
///             | ternary-expr;
///     </code>
/// </summary>
[Branch]
public partial class InfixExpr : Node {
    protected InfixExpr(Node? parent) : base(parent) { }

    internal static InfixExpr Parse(Node parent) {
        return ParseInfix(0, parent);
    }

    static InfixExpr ParseInfix(int precedence, Node parent) {
        var s = parent.Unit.TokenStream;
        InfixExpr leftExpr = PrefixExpr.Parse(parent);
        if (leftExpr is IDefinitionExpr
         || s.Peek.Type.IsCloseBracket()
         || s.PeekIs(Comma)) {
            return leftExpr;
        }

        // expr (keyword | expr) expr?
        var macro = new MacroMatchExpr(parent).Parse(leftExpr);
        if (macro.Macro != null) {
            return macro;
        }

        while (true) {
            var newPrecedence = -1;
            if (s.Peek is OperatorToken opToken) {
                newPrecedence = opToken.Precedence;
            }
            // NOTE: this condition disallows identifiers
            //  on newline to be used as operators.
            else if (!s.Token.Is(Newline, Outdent) && s.PeekIs(Identifier)) {
                newPrecedence = 4;
            }

            if (newPrecedence < precedence) {
                break;
            }

            s.EatAny();
            leftExpr = new BinaryExpr(parent) {
                Left     = leftExpr,
                Operator = s.Token,
                Right    = ParseInfix(newPrecedence + 1, parent)
            };
        }
        if (s.Token.Is(Newline, Outdent)) {
            return leftExpr;
        }

        if (s.PeekIs(KeywordFor)) {
            leftExpr = new ForComprehension(parent) {
                Target = leftExpr
            }.Parse();
        }
        if (s.PeekIs(KeywordIf, KeywordUnless)) {
            return new TernaryExpr(parent) {
                TrueExpr = leftExpr
            }.Parse();
        }

        return leftExpr;
    }
}
