using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree {
    public class OperationDefinition : Token {
        public readonly Token         LeftOperand;
        public readonly OperatorToken OperatorToken;
        public readonly Token         RightOperand;

        public OperationDefinition(OperatorToken @operator, Token leftOperand, Token rightOperand) :
            base(
                TokenType.Identifier,
                (leftOperand?.StartLine ?? @operator.StartLine,
                 leftOperand?.StartColumn ?? @operator.StartColumn),
                @operator.Value
            ) {
            OperatorToken = @operator;
            LeftOperand   = leftOperand;
            RightOperand  = rightOperand;
        }
    }
}