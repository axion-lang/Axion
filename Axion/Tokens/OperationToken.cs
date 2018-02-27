using System.Text;

namespace Axion.Tokens
{
	internal class OperationToken : Token
	{
		internal readonly string Operator;
		internal readonly Token LeftOperand;
		internal readonly Token RightOperand;

		internal OperationToken(string op, Token leftOperand, Token rightOperand)
		{
			Operator = op;
			LeftOperand = leftOperand;
			RightOperand = rightOperand;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (int i = 0; i < tabLevel; i++)
			{
				tabs += "    ";
			}

			var str = new StringBuilder($"{tabs}(Operation,\r\n" +
			                            $"{tabs}    '{Operator ?? "Unknown"}',\r\n");
			str.AppendLine($"{LeftOperand.ToString(tabLevel + 1)},");
			str.AppendLine($"{RightOperand.ToString(tabLevel + 1)}");
			str.Append($"{tabs})");
			return str.ToString();
		}
	}
}
