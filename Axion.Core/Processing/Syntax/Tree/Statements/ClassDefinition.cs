using Axion.Core.Processing.Syntax.Tree.Expressions;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements {
    public class ClassDefinition : Statement {
        [JsonProperty]
        public NameExpression Name { get; }

        [JsonProperty]
        public Expression[] Bases { get; }

        [JsonProperty]
        public Expression[] Keywords { get; }

        [JsonProperty]
        public Expression MetaClass { get; set; }

        [JsonProperty]
        public Statement Body { get; set; }

        //public Expression[] Decorators { get; internal set; }

        public ClassDefinition(NameExpression name, Expression[] bases, Expression[] keywords, Statement body = null, Expression metaClass = null) {
            Name      = name;
            Bases     = bases;
            Keywords  = keywords;
            Body      = body;
            MetaClass = metaClass;
        }
    }
}