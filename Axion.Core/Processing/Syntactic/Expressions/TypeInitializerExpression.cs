using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    /// TODO: complete new_expr
    ///         new_expr:
    ///             'new' type ['(' arglist ')'] ['{' '}']
    ///     </c>
    /// </summary>
    public class TypeInitializerExpression : Expression {
        private TypeName val;

        public TypeName Value {
            get => val;
            set => SetNode(ref val, value);
        }

        private NodeList<CallArgument> args;

        public NodeList<CallArgument> Args {
            get => args;
            set => SetNode(ref args, value);
        }

        internal TypeInitializerExpression(SyntaxTreeNode parent) : base(parent) {
            MarkStart(TokenType.KeywordNew);
            Value = TypeName.ParseTypeName(this);
            if (MaybeEat(TokenType.OpenParenthesis)) {
                Args = CallArgument.ParseArgList(this);
            }

            if (MaybeEat(TokenType.OpenBrace)) {
                Eat(TokenType.CloseBrace);
            }

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("new " + Value + "(");
            c.AddJoin(", ", Args);
            c.Write(")");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("new " + Value + "(");
            c.AddJoin(", ", Args);
            c.Write(")");
        }
    }
}