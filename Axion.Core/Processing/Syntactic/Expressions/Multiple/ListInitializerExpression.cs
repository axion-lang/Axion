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

        internal ListInitializerExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.OpenBracket);
            Expression expr = ParseExpression(this, expectedTypes: Spec.TestExprs);
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

        public ListInitializerExpression(NodeList<Expression> expressions) {
            Expressions = expressions ?? new NodeList<Expression>(this);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.WriteLine("[");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("]");
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.WriteLine("{");
            c.AddJoin("", Expressions, true);
            c.WriteLine();
            c.Write("}");
        }
    }
}