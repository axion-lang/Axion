using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Atomic {
    public class CodeQuoteExpression : Expression {
        private Expression val;

        public Expression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public override TypeName ValueType => Value.ValueType;

        internal CodeQuoteExpression(AstNode parent) : base(parent) {
            MarkStartAndEat(TokenType.DoubleOpenBrace);

            Value = ParseVarExpr(this);

            MarkEndAndEat(TokenType.DoubleCloseBrace);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("{{", Value, "}}");
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            throw new NotImplementedException();
        }
    }
}