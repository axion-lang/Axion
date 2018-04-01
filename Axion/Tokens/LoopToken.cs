using System.Collections.Generic;
using System.Text;

namespace Axion.Tokens
{
	internal class LoopToken : Token
	{
		internal OperationToken Indexer;
		internal List<OperationToken> Conditions;
		internal Token IteratorFunction;
		internal List<Token> Block;

		public LoopToken(OperationToken indexer, List<OperationToken> conditions, Token iteratorFunction, List<Token> block)
		{
			Indexer = indexer;
			Conditions = conditions;
			IteratorFunction = iteratorFunction;
			Block = block;
		}

		public override string ToString(int tabLevel)
		{
			var tabs = "";
			for (var i = 0; i < tabLevel; i++)
			{
				tabs += "  ";
			}

			var str = new StringBuilder();
			str.AppendLine($"{tabs}(Loop,");

			if (IteratorFunction != null)
			{
				str.AppendLine($"{tabs}  (Indexer,");
				str.AppendLine(Indexer.ToString(tabLevel + 2));
				str.AppendLine($"{tabs}  ),");
			}

			str.AppendLine($"{tabs}  (Conditions,");
			for (var i = 0; i < Conditions.Count; i++)
			{
				str.AppendLine(Conditions[i].ToString(tabLevel + 2));
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

			if (IteratorFunction != null)
			{
				str.AppendLine($"{tabs}  (IteratorFunction,");
				str.AppendLine(IteratorFunction.ToString(tabLevel + 2));
				str.AppendLine($"{tabs}  ),");
			}

			str.AppendLine($"{tabs}  (Block,");
			for (var i = 0; i < Block.Count; i++)
			{
				str.Append(Block[i].ToString(tabLevel + 2));
				if (i != Block.Count - 1)
				{
					str.AppendLine(",");
				}
				else
				{
					str.AppendLine();
				}
			}
			str.AppendLine($"{tabs}  )");

			return str.ToString();
		}
	}
}