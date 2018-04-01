using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class FunctionCallToken : Token
	{
		internal readonly List<Token> ArgumentsTokens;
		internal readonly Token NameToken;

		internal FunctionCallToken(Token nameToken, List<Token> argumentTokens)
		{
			LinePosition = nameToken.LinePosition;
			ColumnPosition = nameToken.ColumnPosition;
			Type = TokenType.Identifier;
			NameToken = nameToken;
			ArgumentsTokens = argumentTokens;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Call,");
			str.AppendLine($"{tabs}  '{NameToken.Value ?? "Unknown"}',");
			str.AppendLine($"{tabs}  (");
			for (var i = 0; i < ArgumentsTokens.Count; i++)
			{
				str.Append(ArgumentsTokens[i].ToString(tabLevel + 2));
				if (i != ArgumentsTokens.Count - 1)
				{
					str.AppendLine(",");
				}
				else
				{
					str.AppendLine();
				}
			}

			str.AppendLine($"{tabs}  )");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}