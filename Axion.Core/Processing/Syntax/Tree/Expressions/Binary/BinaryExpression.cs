using System;
using System.CodeDom;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification.CSharp;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.Binary {
    public class BinaryExpression : LeftRightExpression {
        [JsonProperty]
        internal OperatorToken Operator { get; }

        public BinaryExpression(Expression left, OperatorToken op, Expression right) {
            Left     = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right    = right ?? throw new ArgumentNullException(nameof(right));

            MarkPosition(left, right);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Left + " " + Operator.Value + " " + Right;
        }

        internal override CodeObject ToCSharp() {
            if (!Spec.CSharpBinaryOperators.TryGetValue(
                Operator.Properties.Type,
                out CodeBinaryOperatorType csType
            )) {
                SourceUnit.ReportError(
                    "The '" + Operator.Value + "' is not implemented for C# language",
                    Operator
                );
            }

            return new CodeBinaryOperatorExpression(
                (CodeExpression) Left.ToCSharp(),
                csType,
                (CodeExpression) Right.ToCSharp()
            );
        }
    }
}