using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         list_expr:
    ///             '[' comprehension | (expr {',' expr} [',']) ']'
    ///     </c>
    /// </summary>
    public class ListInitializerExpression : MultipleExpression<Expression> {
        internal override TypeName ValueType => Spec.ListType(Expressions[0].ValueType);

        public ListInitializerExpression(SyntaxTreeNode parent, NodeList<Expression> expressions) : base(parent) {
            Expressions = expressions ?? new NodeList<Expression>(this);
        }

        internal ListInitializerExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.OpenBracket);
            Expression expr = ParseMultiple(this, expectedTypes: Spec.TestExprs);
            // unpack multiple expressions wrapped in tuple
            if (expr is TupleExpression t) {
                Expressions = t.Expressions;
            }
            else {
                Expressions = new NodeList<Expression>(this) {
                    expr
                };
            }
            Eat(TokenType.CloseBracket);
            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("[");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("]");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("}");
        }
    }
}