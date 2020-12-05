using System;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    public static class Auxiliary {
        public static Func<Node, Expr> GetParsingFunction<T>() where T : Expr {
            Func<Node, Expr> parserFunc = AnyExpr.Parse;
            if (Spec.ParsingFunctions.TryGetValue(
                typeof(T).Name,
                out var specialParsingFunc
            )) {
                parserFunc = specialParsingFunc;
            }

            return parserFunc;
        }
    }
}
