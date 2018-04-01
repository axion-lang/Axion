namespace Axion.Tokens
{
	internal class NumberToken : Token
	{
		public readonly NumberType NumberType;

		internal NumberToken(NumberType numberType, string value, int linePosition = 0, int columnPosition = 0)
		{
			Type = TokenType.Number;
			NumberType = numberType;
			Value = value;
			LinePosition = linePosition;
			ColumnPosition = columnPosition;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			return $"{tabs}(Number_{NumberType:G}, \'{Value}\')";
		}
	}
}