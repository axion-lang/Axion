using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class CollectionToken : Token
	{
		internal readonly CollectionType CollectionType;
		internal readonly List<Token>    ItemsTokens;
		internal readonly Token          ItemType;

		internal CollectionToken(Token itemType, CollectionType collectionType, List<Token> itemsTokens)
		{
			LinePosition   = itemType.LinePosition;
			ColumnPosition = itemType.ColumnPosition;
			Type           = TokenType.Identifier;
			ItemType       = itemType;
			CollectionType = collectionType;
			ItemsTokens    = itemsTokens;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Collection,");
			str.AppendLine($"{tabs}  '{CollectionType:G}',");
			str.AppendLine($"{ItemType?.ToString(tabLevel + 1) ?? "  Unknown"},");
			str.AppendLine($"{tabs}  (\r\n");
			for (var i = 0; i < ItemsTokens.Count; i++)
			{
				if (i == ItemsTokens.Count - 1)
				{
					str.AppendLine($"{ItemsTokens[i].ToString(tabLevel + 2)}");
					break;
				}

				str.AppendLine($"{ItemsTokens[i].ToString(tabLevel + 2)},");
			}

			str.AppendLine($"{tabs}  )");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}