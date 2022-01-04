using System;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Magnolia.Trees;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions.Generic;

/// <summary>
///     <code>
///         ('(' inner ')') | inner;
///         inner:
///             %expr {',' %expr}
///     </code>
///     Provides functions for comma-separated expression lists
///     parsing with automatic error reporting.
/// </summary>
public class Multiple : AtomExpr {
    protected Multiple(Node parent) : base(parent) { }

    /// <summary>
    ///     Parses multiple of <see cref="AnyExpr" /> and
    ///     reports errors if any of them is not compliant to <see cref="T" />.
    /// </summary>
    internal static AtomExpr ParsePermissively<T>(Node parent)
        where T : Node {
        var e = Parse<Node>(parent);
        if (e is TupleExpr tpl) {
            foreach (var expr in tpl.Expressions) {
                if (expr is not T) {
                    LanguageReport.UnexpectedType(typeof(T), expr);
                }
            }
        }
        else {
            if (e is not T) {
                LanguageReport.UnexpectedType(typeof(T), e);
            }
        }

        return e;
    }

    /// <summary>
    ///     Helper for parsing multiple comma-separated
    ///     expressions with optional parenthesis
    ///     (e.g. tuples)
    /// </summary>
    internal static AtomExpr Parse<T>(Node parent) where T : Node {
        var parserFunc = Auxiliary.GetParsingFunction<T>();
        return Parse(parent, parserFunc);
    }

    /// <summary>
    ///     Helper for parsing multiple comma-separated
    ///     expressions with optional parenthesis
    ///     (e.g. tuples)
    /// </summary>
    internal static AtomExpr Parse(Node parent, Func<Node, Node> parserFunc) {
        var s = parent.Unit.TokenStream;
        var hasParens = s.MaybeEat(OpenParenthesis);
        var list = new NodeList<Node, Ast>(parent) {
            parserFunc(parent)
        };
        // tuple
        if (hasParens) {
            while (s.MaybeEat(Comma) && !s.PeekIs(CloseParenthesis)) {
                list += parserFunc(parent);
            }
            s.Eat(CloseParenthesis);
        }
        else {
            while (s.MaybeEat(Comma)) {
                list += parserFunc(parent);
            }
        }

        return new TupleExpr(parent) {
            Expressions = list
        };
    }
}
