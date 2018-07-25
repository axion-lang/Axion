namespace Axion.Tokens.Ast {
    public class OperationDefinition : Token {
        public readonly Token         LeftOperand;
        public readonly OperatorToken OperatorToken;
        public readonly Token         RightOperand;

        public OperationDefinition(OperatorToken @operator, Token leftOperand, Token rightOperand) :
            base(
                TokenType.Identifier,
                (leftOperand?.StartLnPos
              ?? @operator.StartLnPos,
                 leftOperand?.StartClPos
              ?? @operator.StartClPos),
                @operator.Value
            ) {
            OperatorToken = @operator;
            LeftOperand   = leftOperand;
            RightOperand  = rightOperand;
        }
    }
}