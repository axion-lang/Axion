using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements;
using Axion.Core.Processing.Syntactic.Statements.Definitions;

namespace Axion.Core.Processing.Syntactic.Expressions {
    public class LambdaExpression : Expression {
        private TypeName returnType;

        public TypeName ReturnType {
            get => returnType;
            set => SetNode(ref returnType, value);
        }

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

        public LambdaExpression(
            BlockStatement               block,
            NodeList<FunctionParameter>? parameters = null
        ) {
            Block      = block;
            Parameters = parameters ?? new NodeList<FunctionParameter>(this);
        }

        public override void ToAxionCode(CodeBuilder c) {
            throw new NotSupportedException();
        }

        public override void ToCSharpCode(CodeBuilder c) {
            throw new NotSupportedException();
        }
    }
}