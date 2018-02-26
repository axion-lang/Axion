using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class FunctionCallToken : Token
	{
		internal readonly Token FunctionToken;
		internal readonly List<Token> ArgumentTokens;

		internal FunctionCallToken(Token functionToken, List<Token> argumentTokens)
		{
			Type = TokenType.Identifier;
			FunctionToken = functionToken;
			ArgumentTokens = argumentTokens;
		}

		public override string ToString()
		{
			var str = new StringBuilder($"(Call,\r\n    {FunctionToken?.ToString() ?? "Unknown"},\r\n    (\r\n");
			if (ArgumentTokens.Count != 0)
			{
				for (int i = 0; i < ArgumentTokens.Count; i++)
				{
					var token = ArgumentTokens[i];
					if (token is OperationToken opToken)
					{
						str.AppendLine("        (Operation,");
						str.AppendLine($"            '{opToken.Operator ?? "Unknown"}',");
						str.AppendLine($"            {opToken.LeftOperand?.ToString() ?? "Null"},");
						str.AppendLine($"            {opToken.RightOperand?.ToString() ?? "Null"}");
						str.Append("        )");
					}
					else
					{
						str.Append($"        {token}");
					}

					if (i != ArgumentTokens.Count - 1)
					{
						str.AppendLine(",");
					}
				}
				str.AppendLine();
			}

			str.Append("    )\r\n)");
			return str.ToString();
		}
	}
}
