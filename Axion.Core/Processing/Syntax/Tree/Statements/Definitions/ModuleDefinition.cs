using System;
using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class ModuleDefinition : Statement, IDecorated {
        private Expression name;

        public Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private BlockStatement block;

        public BlockStatement Block {
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

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "module " + Name + " " + Block;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + "namespace " + Name + " " + Block;
        }
    }
}