using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class FunctionCallToken : Token
	{
		internal readonly Token FuncNameToken;
		internal readonly List<Token> ArgumentTokens;

		internal FunctionCallToken(Token functionToken, List<Token> argumentTokens)
		{
			Type = TokenType.Identifier;
			FuncNameToken = functionToken;
			ArgumentTokens = argumentTokens;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (int i = 0; i < tabLevel; i++)
			{
				tabs += "    ";
			}

			var str = new StringBuilder($"{tabs}(Call,\r\n" +
			                            $"{tabs}    '{FuncNameToken.Value ?? "Unknown"}',\r\n" +
			                            $"{tabs}    (\r\n");
			if (ArgumentTokens.Count != 0)
			{
				for (int i = 0; i < ArgumentTokens.Count; i++)
				{
					if (i == ArgumentTokens.Count - 1)
					{
						str.AppendLine($"{ArgumentTokens[i].ToString(tabLevel + 2)}");
						break;
					}
					str.AppendLine($"{ArgumentTokens[i].ToString(tabLevel + 2)},");
				}
			}

			str.Append($"{tabs}    )\r\n" +
			           $"{tabs})");
			return str.ToString();
		}
	}
}
