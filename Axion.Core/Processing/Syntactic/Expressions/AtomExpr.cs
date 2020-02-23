using System.Linq;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Definitions;
using Axion.Core.Specification;
using static Axion.Core.Processing.Lexical.Tokens.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         atom
    ///             : name
    ///             | await_expr
    ///             | parenthesis_expr
    ///             | CONSTANT;
    ///     </c>
    /// </summary>
    public static class AtomExpr {
        internal static Expr Parse(Expr parent) {
            TokenStream s = parent.Source.TokenStream;

            switch (s.Peek.Type) {
            case Identifier: {
                return new NameExpr(parent).Parse(true);
            }

            case KeywordAwait: {
                return new AwaitExpr(parent).Parse();
            }

            case KeywordFn: {
                return new FunctionDef(parent).Parse(true);
            }

            case DoubleOpenBrace: {
                return new CodeQuoteExpr(parent).Parse();
            }

            case Dollar: {
                return new EBNFSyntaxExpr(parent).Parse();
            }

            case OpenParenthesis: {
                // empty tuple
                if (s.PeekByIs(2, CloseParenthesis)) {
                    return new TupleExpr(parent).ParseEmpty();
                }

                return Parsing.MultipleExprs(parent);
            }

            default: {
                if (Spec.Constants.Contains(s.Peek.Type)) {
                    return new ConstantExpr(parent).Parse();
                }

                MacroApplicationExpr macro = new MacroApplicationExpr(parent).Parse();
                if (macro.MacroDef != null) {
                    return macro;
                }

                break;
            }
            }

            return new UnknownExpr(parent).Parse();
        }
    }
}