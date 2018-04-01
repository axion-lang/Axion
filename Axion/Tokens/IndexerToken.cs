using System.Text;

namespace Axion.Tokens
{
	internal class IndexerToken : Token
	{
		internal readonly Token Index;
		internal readonly Token Parent;

		internal IndexerToken(Token parent, Token index)
		{
			LinePosition = parent.LinePosition;
			ColumnPosition = parent.ColumnPosition;
			Type = TokenType.Identifier;
			Parent = parent;
			Index = index;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Indexer,");
			str.AppendLine($"{Parent?.ToString(tabLevel + 1) ?? $"{tabs}Unknown"},");
			str.AppendLine($"{Index?.ToString(tabLevel + 1) ?? $"{tabs}Unknown"},");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}