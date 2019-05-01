using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Binary;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    /// <summary>
    ///     <c>
    ///         type_initializer_expr:
    ///             'new' type ['(' arg_list ')'] ['{' '}']
    ///     </c>
    /// </summary>
    public class TypeInitializerExpression : Expression {
        private NodeList<CallArgument> args;

        public NodeList<CallArgument> Args {
            get => args;
            set => SetNode(ref args, value);
        }

        private NodeList<Expression> initParameters;

        public NodeList<Expression> InitParameters {
            get => initParameters;
            set => SetNode(ref initParameters, value);
        }

        private TypeName valueType;

        public sealed override TypeName ValueType {
            get {
                if (valueType == null) {
                    if (Parent is BinaryOperationExpression bin
                        && bin.Operator.Type == OpAssign) {
                        return bin.Left.ValueType;
                    }

                    Unit.ReportError(
                        Spec.CannotInferTypeError(typeof(TypeInitializerExpression)),
                        this
                    );
                }

                return valueType;
            }
            set => SetNode(ref valueType, value);
        }

        internal TypeInitializerExpression(SyntaxTreeNode parent) : base(parent) {
            Args           = new NodeList<CallArgument>(this);
            InitParameters = new NodeList<Expression>(this);
            EatStartMark(KeywordNew);
            if (Peek.Is(Identifier)) {
                ValueType = TypeName.ParseTypeName(this);
            }

            if (MaybeEat(OpenParenthesis)) {
                Args = CallArgument.ParseArgList(this);
            }

            if (MaybeEat(OpenBrace)) {
                do {
                    InitParameters.Add(ParsePreGlobalExpr(this));
                } while (MaybeEat(Comma));

                Eat(CloseBrace);
            }

            MarkEnd(Token);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("new " + ValueType + "(");
            c.AddJoin(", ", Args);
            c.Write(")");
            if (InitParameters.Count > 0) {
                c.WriteLine(" {");
                c.AddJoin("", InitParameters, true);
                c.Write("}");
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("new " + ValueType + "(");
            c.AddJoin(", ", Args);
            c.Write(")");
            if (InitParameters.Count > 0) {
                c.WriteLine(" {");
                c.AddJoin("", InitParameters, true);
                c.Write("}");
            }
        }
    }
}