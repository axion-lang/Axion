using System;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.Patterns;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic;

public static class Auxiliary {
    public static Func<Node, Node> GetParsingFunction<T>() where T : Node {
        var parserFunc = AnyExpr.Parse;
        if (ExpressionPattern.ParsingFunctions.TryGetValue(
                typeof(T).Name,
                out var specialParsingFunc
            )) {
            parserFunc = specialParsingFunc;
        }

        return parserFunc;
    }
}
