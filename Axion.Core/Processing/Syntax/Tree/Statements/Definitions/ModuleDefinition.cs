using System;
using System.CodeDom;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class ModuleDefinition : Statement, IDecorated, ITopLevelDefinition {
        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private BlockStatement block;

        [JsonProperty]
        internal BlockStatement Block {
            get => block;
            set {
                if (value != null) {
                    value.Parent = this;
                }
                block = value;
            }
        }

        public List<Expression> Modifiers { get; set; }

        internal ModuleDefinition(Expression name, BlockStatement block) {
            Name  = name ?? throw new ArgumentNullException(nameof(name));
            Block = block ?? throw new ArgumentNullException(nameof(block));
            MarkPosition(name, block);
        }

        internal override CodeObject ToCSharp() {
            var ns = new CodeNamespace(Name.ToString());

            foreach (Statement s in Block.Statements) {
                if (s is ClassDefinition || s is EnumDefinition) {
                    ns.Types.Add((CodeTypeDeclaration) s.ToCSharp());
                }
            }
            return ns;
        }
    }
}