using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axion.Tokens
{
	internal class BranchingToken : Token
	{
		internal readonly List<OperationToken> Conditions;
		internal readonly List<Token> ElseBlock;
		internal readonly Dictionary<List<OperationToken>, List<Token>> ElseIfs;
		internal readonly List<Token> ThenBlock;

		internal BranchingToken(List<OperationToken> conditions, List<Token> thenBlock)
		{
			Conditions = conditions;
			ThenBlock = thenBlock;
			ElseIfs = new Dictionary<List<OperationToken>, List<Token>>();
			ElseBlock = new List<Token>();
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(If,");
			str.AppendLine($"{tabs}  (Conditions,");
			for (var i = 0; i < Conditions.Count; i++)
			{
				str.Append(Conditions[i].ToString(tabLevel + 2));
				if (i != Conditions.Count - 1)
				{
					str.AppendLine(",");
				}
				else
				{
					str.AppendLine();
				}
			}

			str.AppendLine($"{tabs}  ),");
			str.AppendLine($"{tabs}  (Then,");
			for (var i = 0; i < ThenBlock.Count; i++)
			{
				str.Append(ThenBlock[i].ToString(tabLevel + 2));
				if (i != ThenBlock.Count - 1)
				{
					str.AppendLine(",");
				}
				else
				{
					str.AppendLine();
				}
			}

			str.AppendLine($"{tabs}  ),");

			for (var i = 0; i < ElseIfs.Count; i++)
			{
				str.AppendLine($"{tabs}  (ElseIf,");
				str.AppendLine($"{tabs}    (Conditions,");
				List<OperationToken> elifConditions = ElseIfs.ElementAt(i).Key;
				for (var I = 0; I < elifConditions.Count; I++)
				{
					str.Append(elifConditions[I].ToString(tabLevel + 3));
					if (I != elifConditions.Count - 1)
					{
						str.AppendLine(",");
					}
					else
					{
						str.AppendLine();
					}
				}

				str.AppendLine($"{tabs}    ),");
				str.AppendLine($"{tabs}    (Then,");
				List<Token> elifBlock = ElseIfs.ElementAt(i).Value;
				for (var I = 0; I < elifBlock.Count; I++)
				{
					str.Append(elifBlock[I].ToString(tabLevel + 3));
					if (I != elifBlock.Count - 1)
					{
						str.AppendLine(",");
					}
					else
					{
						str.AppendLine();
					}
				}

				str.AppendLine($"{tabs}    ),");
				str.AppendLine($"{tabs}  ),");
			}

			str.AppendLine($"{tabs}  (Else,");
			for (var i = 0; i < ElseBlock.Count; i++)
			{
				str.Append(ElseBlock[i].ToString(tabLevel + 2));
				if (i != ElseBlock.Count - 1)
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