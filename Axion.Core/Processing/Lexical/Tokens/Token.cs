using System.Web;
using Axion.Core.Hierarchy;
using Axion.Specification;
using Magnolia.Attributes;
using Newtonsoft.Json;
using static Axion.Specification.TokenType;

namespace Axion.Core.Processing.Lexical.Tokens;

[Branch]
public partial class Token : Node {
    [JsonProperty(Order = 1)]
    public TokenType Type { get; init; }

    [JsonProperty(Order = 2)]
    public string Value { get; init; }

    [JsonProperty(Order = 3)]
    public string Content { get; init; }

    [JsonProperty(Order = 4)]
    public string EndingWhite { get; set; }

    public Token(
        Unit      unit,
        TokenType type        = None,
        string    value       = "",
        string?   content     = null,
        string    endingWhite = "",
        Location  start       = default,
        Location  end         = default
    ) : base(unit, start, end) {
        Type        = type;
        Value       = value;
        Content     = content ?? value;
        EndingWhite = endingWhite;
    }

    public bool Is(params TokenType[] types) {
        if (types.Length == 0) {
            return true;
        }

        var t = Type;
        for (var i = 0; i < types.Length; i++) {
            if (t == types[i]) {
                return true;
            }
        }

        return false;
    }

    internal void MarkStart(Location start) {
        Start = start;
    }

    internal void MarkEnd(Location end) {
        End = end;
    }

    public override string ToString() {
        return Type
             + " :: "
             + HttpUtility.JavaScriptStringEncode(Value)
             + " :: "
             + base.ToString();
    }
}
