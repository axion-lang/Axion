using System.Text;

namespace Axion.Tokens
{
	internal class IndexerToken : Token
    {
	    internal readonly Token Parent;
	    internal readonly Token Index;

	    internal IndexerToken(Token parent, Token index)
	    {
		    Type = TokenType.Identifier;
		    Parent = parent;
		    Index = index;
	    }

	    public override string ToString(int tabLevel)
	    {
		    var tabs = "";
		    for (int i = 0; i < tabLevel; i++)
		    {
			    tabs += "    ";
		    }

		    var str = new StringBuilder();
		    str.AppendLine($"{tabs}(Indexer,");
		    str.AppendLine($"{Parent?.ToString(tabLevel + 1) ?? $"{tabs}Unknown"},");
		    str.AppendLine($"{Index?.ToString(tabLevel + 1) ?? $"{tabs}Unknown"},");
		    str.Append($"{tabs})");
		    return str.ToString();
	    }
	}
}
