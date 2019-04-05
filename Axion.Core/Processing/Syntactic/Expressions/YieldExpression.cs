using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         yield_expr ::=
    ///             'yield' ['from' test | test_list]
    ///     </c>
    /// </summary>
    public class YieldExpression : Expression {
        private  Expression val;

        [NotNull]
        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }
        
        internal bool IsYieldFrom { get; }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        internal YieldExpression(
            Expression value,
            bool       isYieldFrom,
            Token      start,
            Token      end
        ) {
            Value       = value ?? new ConstantExpression(TokenType.KeywordNil);
            IsYieldFrom = isYieldFrom;
            MarkPosition(start, end);
        }

        internal YieldExpression(SyntaxTreeNode parent) {
            Parent = parent;
            StartNode(TokenType.KeywordYield);
            // Mark that function as generator.
            // If we're in a generator expr, then we don't have a function yet.
            // g = ((yield i) for i in range(5))
            // In that case, the gen_expr will mark IsGenerator. 
            if (Unit.Ast.currentFunction != null) {
                Unit.Ast.currentFunction.IsGenerator = true;
            }

            // Parse expr list after yield. This can be:
            // 1) empty, in which case it becomes 'yield nil'
            // 2) a single expr
            // 3) multiple expressions, in which case it's wrapped in a tuple.
            if (MaybeEat(TokenType.KeywordFrom)) {
                Value       = ParseTestExpr(this);
                IsYieldFrom = true;
            }
            else {
                Value = ParseExpression(this) ?? new ConstantExpression(TokenType.KeywordNil);
            }

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "yield " + Value;
        }
    }
}