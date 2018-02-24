namespace Axion.Tokens
{
    internal class OperationToken : Token
    {
        internal readonly string Operator;
        internal readonly Token LeftOperand;
        internal readonly Token RightOperand;

        internal OperationToken(string op, Token leftOperand, Token rightOperand)
        {
            Operator = op;
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override string ToString()
        {
            return "[Operation,\r\n" +
                   $"    \"{Operator ?? "Unknown"}\",\r\n" +
                   $"    {LeftOperand?.ToString() ?? "Unknown"},\r\n" +
                   $"    {RightOperand?.ToString() ?? "Unknown"}\r\n],";
        }
    }
}
