namespace Axion.Tokens
{
	internal class Token
	{
		internal int ColumnPosition;
		internal int LinePosition;
		public TokenType Type = TokenType.Unknown;
		public string Value;

		internal Token()
		{
		}

		internal Token(string value, int linePosition = 0, int columnPosition = 0, TokenType type = TokenType.Unknown)
		{
			Type = type;
			Value = value;
			LinePosition = linePosition;
			ColumnPosition = columnPosition;
		}

		public override string ToString()
		{
			return ToString(0);
		}

		public virtual string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			return Type == TokenType.Unknown
				? $"{tabs}({Value.Escape()})"
				: $"{tabs}({Type:G}, '{Value.Escape()}')";
		}
	}
}