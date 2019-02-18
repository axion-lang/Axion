using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class ClassDefinition : Statement, IDecorated, ITopLevelDefinition {
        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private TypeName[] bases;

        [JsonProperty]
        internal TypeName[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (TypeName expr in bases) {
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
                if (value != null) {
                    value.Parent = this;
                }
                metaClass = value;
            }
        }

        private Statement block;

        [JsonProperty]
        internal Statement Block {
            get => block;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                block = value;
            }
        }

        public List<Expression> Modifiers { get; set; }

        public ClassDefinition(
            Expression   name,
            TypeName[]   bases,
            Expression[] keywords,
            Statement    block     = null,
            Expression   metaClass = null
        ) {
            Name      = name;
            Bases     = bases;
            Keywords  = keywords;
            Block     = block;
            MetaClass = metaClass;
        }
    }
}