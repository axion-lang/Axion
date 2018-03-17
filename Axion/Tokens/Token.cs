namespace Axion.Tokens
{
	internal class Token
	{
		internal int       ColumnPosition;
		internal int       LinePosition;
		public   TokenType Type = TokenType.Unknown;
		public   string    Value;

		internal Token() { }

		internal Token(TokenType type, string value = null, int linePosition = 0, int columnPosition = 0)
		{
			Type           = type;
			Value          = value;
			LinePosition   = linePosition;
			ColumnPosition = columnPosition;
		}

		public virtual string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			return Value == null ? $"{tabs}({Type})" : $"{tabs}({Type:G}, '{Value}')";
		}
	}
}