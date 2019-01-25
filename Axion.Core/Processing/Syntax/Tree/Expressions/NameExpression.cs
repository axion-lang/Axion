using System.Diagnostics;
using System.Linq;
using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class NameExpression : Expression {
        [JsonProperty]
        public string Name => ToAxionCode();

        public readonly Token[] NameParts;

        public readonly bool IsSimple;

        public NameExpression(params Token[] nameParts) {
            Debug.Assert(nameParts.Length > 0);

            NameParts = nameParts;
            IsSimple  = nameParts.Length == 1;
            MarkPosition(nameParts[0], nameParts[nameParts.Length - 1]);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return string.Join(".", NameParts.Select(t => t.Value));
        }
    }
}