namespace Axion.Tokens
{
    internal class Token
    {
        public readonly TokenType Type = TokenType.Unknown;
        public readonly string Value;

        internal Token() { }

        internal Token(TokenType type)
        {
            Type = type;
        }

        internal Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return Value == null ? $"({Type})" : $"({Type:G}, '{Value}')";
        }
    }
}
