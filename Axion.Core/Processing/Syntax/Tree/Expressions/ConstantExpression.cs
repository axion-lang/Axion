using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class ConstantExpression : Expression {
        [JsonProperty]
        internal Token Value { get; }

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        internal ConstantExpression(TokenType type) {
            Value = new KeywordToken(type);
        }

        internal ConstantExpression(Token value) {
            Value = value;
            MarkPosition(Value);
        }

        internal ConstantExpression(Token value, Token start, Token end) : base(start, end) {
            Value = value;
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Value;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Value;
        }
    }
}