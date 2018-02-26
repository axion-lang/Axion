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

		public override string ToString()
		{
			var str = new StringBuilder($"(Operation,\r\n    '{Operator ?? "Unknown"}',\r\n");
			if (LeftOperand is OperationToken opToken)
			{
				str.AppendLine("    (Operation,");
				str.AppendLine($"        '{opToken.Operator ?? "Unknown"}',");
				str.AppendLine($"        {opToken.LeftOperand?.ToString() ?? "Null"},");
				str.AppendLine($"        {opToken.RightOperand?.ToString() ?? "Null"}");
				str.AppendLine("    ),");
			}
			else
			{
				str.AppendLine($"    {LeftOperand},");
			}
			if (RightOperand is OperationToken opToken2)
			{
				str.AppendLine("    (Operation,");
				str.AppendLine($"        '{opToken2.Operator ?? "Unknown"}',");
				str.AppendLine($"        {opToken2.LeftOperand?.ToString() ?? "Null"},");
				str.AppendLine($"        {opToken2.RightOperand?.ToString() ?? "Null"}");
				str.AppendLine("    )");
			}
			else
			{
				str.AppendLine($"    {RightOperand}");
			}
			str.Append("),");
			return str.ToString();
		}
	}
}
