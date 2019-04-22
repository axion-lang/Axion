using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         yield_expr:
    ///             'yield' ['from' test | test_list]
    ///     </c>
    /// </summary>
    public class YieldExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        internal bool IsYieldFrom { get; }

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

        internal YieldExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordYield);
            // Mark that function as generator.
            // If we're in a generator expr, then we don't have a function yet.
            // g = ((yield i) for i in range(5))
            // In that case, the gen_expr will mark IsGenerator. 
            if (Ast.CurrentFunction != null) { }

            // Parse expr list after yield. This can be:
            // 1) a single expr
            // 2) multiple expressions, in which case it's wrapped in a tuple.
            if (MaybeEat(TokenType.KeywordFrom)) {
                Value       = ParseTestExpr(this);
                IsYieldFrom = true;
            }
            else {
                Value = ParseExpression(this, expectedTypes: Spec.TestExprs);
            }

            MarkEnd(Token);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("yield ", Value);
        }
    }
}