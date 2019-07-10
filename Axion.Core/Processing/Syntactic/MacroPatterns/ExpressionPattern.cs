using System;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class ExpressionPattern : IPattern {
        private readonly Type             type;
        private readonly Func<Expression> parseFunc;

        public ExpressionPattern(Type type) {
            this.type = type;
        }

        public ExpressionPattern(Func<Expression> parseFunc) {
            this.parseFunc = parseFunc;
        }

        public bool Match(Expression parent) {
            int idx = parent.Ast.CurrentTokenIndex;
            // leave expression non-starters to next token pattern.
            if (parent.Peek.Is(Spec.NeverExprStartTypes)) {
                return true;
            }

            Expression e;
            if (parseFunc != null) {
                e = parseFunc();
                parent.Ast.MacroApplicationParts.Add(e);
                return true;
            }

            parent.Ast.MacroExpectationType = type;
            if (typeof(TypeName).IsAssignableFrom(type)) {
                e = new TypeName(parent).ParseTypeName();
            }
            else {
                e = new Expression(parent).ParseAny();
            }

            parent.Ast.MacroExpectationType = null;
            if (type.IsInstanceOfType(e)) {
                parent.Ast.MacroApplicationParts.Add(e);
                return true;
            }

            parent.MoveTo(idx);
            return false;
        }
    }
}