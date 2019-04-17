using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
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

        public ListInitializerExpression(NodeList<Expression> expressions) {
            Expressions = expressions ?? new NodeList<Expression>(this);
        }

        internal ListInitializerExpression(NodeList<Expression> expressions, Token start, Token end)
            : this(expressions) {
            MarkPosition(start, end);
        }

        internal ListInitializerExpression(SyntaxTreeNode parent) {
            Parent      = parent;
            Expressions = new NodeList<Expression>(this);

            MarkStart(TokenType.OpenBracket);
            Expressions.Add(ParseMultiple(this, expectedTypes: Spec.TestExprs));
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