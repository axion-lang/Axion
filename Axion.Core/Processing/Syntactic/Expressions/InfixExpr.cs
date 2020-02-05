using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Operations;
using Axion.Core.Processing.Syntactic.Expressions.Postfix;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         infix_expr:
    ///             prefix_expr (ID | SYMBOL) infix_expr;
    ///     </c>
    /// </summary>
    public static class InfixExpr {
        internal static Expr ParseList(Expr parent) {
            return Parsing.MultipleExprs(parent, Parse);
        }

        internal static Expr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            Expr ParseInfix(int precedence) {
                Expr leftExpr = PrefixExpr.Parse(parent);
                if (leftExpr is IDefinitionExpr) {
                    return leftExpr;
                }

                // expr (keyword | expr) expr?
                MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse(leftExpr);
                if (macro.MacroDef != null) {
                    return macro;
                }

                while (true) {
                    int newPrecedence;
                    if (s.Peek is OperatorToken opToken) {
                        newPrecedence = opToken.Precedence;
                    }
                    else if (!s.Token.Is(Newline, Outdent) && s.PeekIs(Identifier)) {
                        newPrecedence = 4;
                    }
                    else {
                        break;
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
                        return new TernaryExpr(parent, trueExpr: leftExpr).Parse();
                    }
                }

                return leftExpr;
            }

            return ParseInfix(0);
        }
    }
}