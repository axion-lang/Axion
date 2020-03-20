using System;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.MacroPatterns {
    public class ExpressionPattern : IPattern {
        private readonly Func<Expr, Expr> parseFunc;
        private readonly Type             type;

        public ExpressionPattern(Type type) {
            this.type = type;
        }

        public ExpressionPattern(Func<Expr, Expr> parseFunc) {
            this.parseFunc = parseFunc;
        }

        public bool Match(Expr parent) {
            // leave expression non-starters to next token pattern.
            if (parent.Stream.PeekIs(Spec.NeverExprStartTypes)) {
                return true;
            }

            Expr e;
            if (parseFunc != null) {
                e = parseFunc(parent);
                parent.Ast.MacroApplicationParts.Peek().Expressions.Add(e);
                return true;
            }

            parent.Ast.MacroExpectType = type;
            if (typeof(TypeName).IsAssignableFrom(type)) {
                e = new TypeName(parent).ParseTypeName();
            }
            else {
                e = AnyExpr.Parse(parent);
            }

            parent.Ast.MacroExpectType = null;
            if (type.IsInstanceOfType(e)) {
                parent.Ast.MacroApplicationParts.Peek().Expressions.Add(e);
                return true;
            }

            return false;
        }
    }
}