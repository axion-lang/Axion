using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    [JsonObject(MemberSerialization.OptIn)]
    public class NameExpression : Expression {
        public readonly Token[] NameParts;

        public readonly bool IsSimple;

        public NameExpression(params Token[] nameParts) {
            NameParts = nameParts;
            IsSimple  = nameParts.Length == 1;
            Name      = ToAxionCode();
            if (nameParts.Length > 0) {
                MarkPosition(nameParts[0], nameParts[nameParts.Length - 1]);
            }
        }

        [JsonProperty]
        public string Name { get; }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return string.Join(".", NameParts.Select(t => t.Value));
        }
    }
}