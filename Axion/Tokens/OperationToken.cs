using System.Linq;
using System.Text;

namespace Axion.Tokens
{
	internal class OperationToken : Token
	{
		internal readonly Token  LeftOperand;
		internal readonly string Operator;
		internal readonly Token  RightOperand;

		internal OperationToken(string op, Token leftOperand, Token rightOperand)
		{
			LinePosition   = leftOperand.LinePosition;
			ColumnPosition = leftOperand.ColumnPosition;
			Operator       = op;
			if (Lexer.ConditionalOperators.Contains(Operator))
			{
				Type = TokenType.Identifier;
			}

			LeftOperand  = leftOperand;
			RightOperand = rightOperand;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Operation,");
			str.AppendLine($"{tabs}  '{Operator ?? "Unknown"}',");
			str.AppendLine($"{LeftOperand?.ToString(tabLevel + 1) ?? $"{tabs}  Unknown"},");
			str.AppendLine($"{RightOperand?.ToString(tabLevel + 1) ?? $"{tabs}  Unknown"}");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}