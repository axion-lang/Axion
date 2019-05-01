using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements;
using Axion.Core.Processing.Syntactic.Statements.Definitions;
using Axion.Core.Processing.Syntactic.Statements.Small;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class LambdaExpression : Expression, IFunctionNode {
        private NodeList<FunctionParameter> parameters;

        public NodeList<FunctionParameter> Parameters {
            get => parameters;
            set => SetNode(ref parameters, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
        }

        public override TypeName ValueType =>
            Spec.FuncType(Parameters.Select(p => p.ValueType), ReturnType);

        public LambdaExpression(SyntaxTreeNode parent) : base(parent) {
            EatStartMark(KeywordFn);
            if (MaybeEat(OpenParenthesis)) {
                Parameters = FunctionDefinition.ParseParameterList(this, CloseParenthesis);
                Eat(CloseParenthesis);
            }
            else if (!Peek.Is(Colon, Indent, OpenBrace, RightFatArrow)) {
                Parameters = FunctionDefinition.ParseParameterList(this, Colon, Indent, OpenBrace);
            }
            else {
                Parameters = new NodeList<FunctionParameter>(this);
            }

            if (MaybeEat(RightFatArrow)) {
                ReturnType = TypeName.ParseTypeName(this);
            }

            Ast.PushFunction(this);
            Block = new BlockStatement(this, BlockType.Lambda);
            Ast.PopFunction();
            if (Block.Statements.Count > 0 && Block.Statements.Last is ExpressionStatement e) {
                Block.Statements.Last = new ReturnStatement(e.Expression);
            }

            MarkEnd(Token);
        }

        public LambdaExpression(
            BlockStatement               block,
            NodeList<FunctionParameter>? parameters = null
        ) {
            Block      = block;
            Parameters = parameters ?? new NodeList<FunctionParameter>(this);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("fn ");
            c.AddJoin(", ", Parameters);
            c.Write(" ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("(");
            c.AddJoin(", ", Parameters);
            c.Write(") => ", Block);
        }
    }
}