using Axion.Core.Processing.Lexical.Tokens;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    [JsonObject(MemberSerialization.OptIn)]
    public class NameExpression : Expression {
        [JsonProperty]
        public IdentifierToken Name { get; }

        public NameExpression(string name) {
            Name = new IdentifierToken(name);
        }

        public NameExpression(IdentifierToken name) {
            Name = name;
            MarkPosition(name);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Name.Value;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Name.Value;
        }
    }
}