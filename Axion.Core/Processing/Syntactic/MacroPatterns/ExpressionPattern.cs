using System;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.MacroPatterns {
    public class ExpressionPattern : IPattern {
        private readonly Type                      type;
        private readonly Func<AstNode, Expression> parseFunc;

        public ExpressionPattern(Type type) {
            this.type = type;
        }

        public ExpressionPattern(Func<AstNode, Expression> parseFunc) {
            this.parseFunc = parseFunc;
        }

        public bool Match(AstNode parent) {
            int idx = parent.Ast.Index;
            // leave expression non-starters to next token pattern.
            if (parent.Peek.Is(Spec.NeverExprStartTypes)) {
                return true;
            }
            Expression e;
            if (parseFunc != null) {
                e = parseFunc(parent);
                parent.Ast.MacroApplicationParts.Add(e);
                return true;
            }
            parent.Ast.MacroExpectationType = type;
            if (typeof(TypeName).IsAssignableFrom(type)) {
                e = TypeName.ParseTypeName(parent);
            }
            else {
                e = Expression.ParseVarExpr(parent);
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