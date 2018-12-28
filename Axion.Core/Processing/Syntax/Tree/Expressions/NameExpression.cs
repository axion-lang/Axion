using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class NameExpression : Expression {
        [JsonProperty]
        public string Name { get; }

        public NameExpression(string name) {
            Name = name;
        }

        public NameExpression(Token name) {
            Name = name.Value;
            MarkPosition(name);
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Name;
        }
    }
}