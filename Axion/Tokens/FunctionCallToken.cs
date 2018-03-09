using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class FunctionCallToken : Token
	{
		internal readonly Token NameToken;
		internal readonly List<Token> ArgumentsTokens;

		internal FunctionCallToken(Token nameToken, List<Token> argumentTokens)
		{
			Type = TokenType.Identifier;
			NameToken = nameToken;
			ArgumentsTokens = argumentTokens;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (int i = 0; i < tabLevel; i++)
			{
				tabs += "    ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Call,");
			str.AppendLine($"{tabs}    '{NameToken.Value ?? "Unknown"}',");
			str.AppendLine($"{tabs}    (");
			for (int i = 0; i < ArgumentsTokens.Count; i++)
			{
				if (i == ArgumentsTokens.Count - 1)
				{
					str.AppendLine($"{ArgumentsTokens[i].ToString(tabLevel + 2)}");
					break;
				}
				str.AppendLine($"{ArgumentsTokens[i].ToString(tabLevel + 2)},");
			}
			str.AppendLine($"{tabs}    )");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}
