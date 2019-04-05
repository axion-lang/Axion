using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.Multiple;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         expr_list ::=
    ///             expr {',' expr} [',']
    ///     </c>
    /// </summary>
    public class TestList : MultipleExpression<Expression> {
        public int Count => Expressions.Count;

        public Expression this[int index] {
            get => Expressions[index];
            set => Expressions[index] = value;
        }
        
        internal TestList(
            SyntaxTreeNode parent,
            out bool       trailingComma
        ) {
            Expressions = new NodeList<Expression>(parent);
            trailingComma = false;
            do {
                Expressions.Add(ParseTestExpr(parent));
                trailingComma = parent.MaybeEat(TokenType.Comma);
            } while (trailingComma && !parent.PeekIs(Spec.NeverTestTypes));
        }

        public void Insert(int index, Expression item) {
            
        }
    }
}