using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class CollectionToken : Token
	{
		internal readonly Token ItemTypeToken;
		internal readonly CollectionType CollectionType;
		internal readonly List<Token> ItemsTokens;

		internal CollectionToken(Token itemTypeToken, CollectionType collectionType, List<Token> itemsTokens)
		{
			Type = TokenType.Identifier;
			ItemTypeToken = itemTypeToken;
			CollectionType = collectionType;
			ItemsTokens = itemsTokens;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (int i = 0; i < tabLevel; i++)
			{
				tabs += "    ";
			}

			var str = new StringBuilder($"{tabs}(Collection,\r\n" +
										$"{tabs}	'{CollectionType:G}',\r\n" +
										$"{tabs}    '{ItemTypeToken?.Value ?? "Unknown"}',\r\n" +
										$"{tabs}    (\r\n");
			for (int i = 0; i < ItemsTokens.Count; i++)
			{
				if (i == ItemsTokens.Count - 1)
				{
					str.AppendLine($"{ItemsTokens[i].ToString(tabLevel + 2)}");
					break;
				}
				str.AppendLine($"{ItemsTokens[i].ToString(tabLevel + 2)},");
			}
			str.Append($"{tabs}    )\r\n" +
					   $"{tabs})");
			return str.ToString();
		}
	}
}
