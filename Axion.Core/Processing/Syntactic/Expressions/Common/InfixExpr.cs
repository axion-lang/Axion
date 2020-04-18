using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Common {
    /// <summary>
    ///     <c>
    ///         infix-expr:
    ///             prefix-expr (ID | SYMBOL) infix-expr;
    ///     </c>
    /// </summary>
    public class InfixExpr : Expr {
        protected InfixExpr() { }

        protected InfixExpr(Expr parent) : base(parent) { }

        internal static InfixExpr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            InfixExpr ParseInfix(int precedence) {
                InfixExpr leftExpr = PrefixExpr.Parse(parent);
                if (leftExpr is IDefinitionExpr
                 || s.Peek.Type.IsCloseBracket()
                 || s.PeekIs(Comma)) {
                    return leftExpr;
                }

                // expr (keyword | expr) expr?
                MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse(leftExpr);
                if (macro.Macro != null) {
                    return macro;
                }

                while (true) {
                    int newPrecedence = -1;
                    if (s.Peek is OperatorToken opToken) {
                        newPrecedence = opToken.Precedence;
                    }
                    // NOTE: this condition disallows identifiers to be used as operators.
                    else if (!s.Token.Is(Newline, Outdent) && s.PeekIs(Identifier)) {
                        newPrecedence = 4;
                    }

                    if (newPrecedence < precedence) {
                        break;
                    }

                    s.EatAny();
                    leftExpr = new BinaryExpr(
                        parent,
                        leftExpr,
                        s.Token,
                        ParseInfix(newPrecedence + 1)
                    );
                }

                if (!s.Token.Is(Newline, Outdent)) {
                    if (s.PeekIs(KeywordFor)) {
                        leftExpr = new ForComprehension(parent, leftExpr).Parse();
                    }

                    if (s.PeekIs(KeywordIf, KeywordUnless)) {
                        return new TernaryExpr(parent) {
                            TrueExpr = leftExpr
                        }.Parse();
                    }
                }

                return leftExpr;
            }

            return ParseInfix(0);
        }
    }
}
