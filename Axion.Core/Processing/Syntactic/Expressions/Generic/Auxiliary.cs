using System;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic {
    public static class Auxiliary {
        public static Func<Expr, Expr> GetParsingFunction<T>()
            where T : Expr {
            Func<Expr, Expr> parserFunc = AnyExpr.Parse;
            if (Spec.ParsingFunctions.TryGetValue(
                typeof(T).Name,
                out Func<Expr, Expr> specialParsingFunc
            )) {
                parserFunc = specialParsingFunc;
            }
            return parserFunc;
        }
    }
}
