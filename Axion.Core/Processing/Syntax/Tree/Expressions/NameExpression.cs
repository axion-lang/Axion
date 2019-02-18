using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    [JsonObject(MemberSerialization.OptIn)]
    public class NameExpression : Expression {
        [JsonProperty]
        public Token Name { get; }

        public NameExpression(Token name) {
            Name = name;
            MarkPosition(name);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Name.ToAxionCode();
        }
    }
}