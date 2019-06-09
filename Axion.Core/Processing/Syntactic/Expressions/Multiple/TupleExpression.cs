using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;

namespace Axion.Core.Processing.Syntactic.Expressions.Multiple {
    /// <summary>
    ///     <c>
    ///         tuple_expr:
    ///             tuple_paren_expr | (expr_list [',']);
    ///         tuple_paren_expr:
    ///             '(' expr_list [','] ')';
    ///     </c>
    /// </summary>
    public class TupleExpression : MultipleExpression {
        public override TypeName ValueType => new TupleTypeName(this, Expressions);

        /// <summary>
        ///     Constructor for empty tuple.
        /// </summary>
        internal TupleExpression(
            AstNode parent,
            Token   start,
            Token   end
        ) : base(parent) {
            Expressions = new NodeList<Expression>(this);
            MarkPosition(start, end);
        }

        internal TupleExpression(
            AstNode              parent,
            NodeList<Expression> expressions
        ) : base(parent) {
            Expressions = expressions;
            if (Expressions.Count > 0) {
                MarkPosition(
                    Expressions[0],
                    Expressions[Expressions.Count - 1]
                );
            }
        }

        public TupleExpression(NodeList<Expression> expressions)
            : base(expressions) { }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Expressions);
            c.Write(")");
        }
    }
}