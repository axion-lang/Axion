using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class ClassDefinition : Statement, IDecorated {
        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private Expression[] bases;

        [JsonProperty]
        internal Expression[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (Expression expr in bases) {
                    expr.Parent = this;
                }
            }
        }

        private Expression[] keywords;

        [JsonProperty]
        internal Expression[] Keywords {
            get => keywords;
            set {
                keywords = value;
                foreach (Expression expr in keywords) {
                    expr.Parent = this;
                }
            }
        }

        private Expression metaClass;

        [JsonProperty]
        internal Expression MetaClass {
            get => metaClass;
            set {
                value.Parent = this;
                metaClass    = value;
            }
        }

        private Statement body;

        [JsonProperty]
        internal Statement Body {
            get => body;
            set {
                value.Parent = this;
                body         = value;
            }
        }

        public List<Expression> Modifiers { get; set; }

        public ClassDefinition(NameExpression name, Expression[] bases, Expression[] keywords, Statement body = null, Expression metaClass = null) {
            Name      = name;
            Bases     = bases;
            Keywords  = keywords;
            Body      = body;
            MetaClass = metaClass;
        }
    }
}