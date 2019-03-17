using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class ClassDefinition : Statement, IDecorated {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                name = value;
            }
        }

        private TypeName[] bases;

        public TypeName[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (TypeName expr in bases) {
                    expr.Parent = this;
                }
            }
        }

        private Expression[] keywords;

        public Expression[] Keywords {
            get => keywords;
            set {
                keywords = value;
                foreach (Expression expr in keywords) {
                    expr.Parent = this;
                }
            }
        }

        private Expression metaClass;

        public Expression MetaClass {
            get => metaClass;
            set {
                if (value != null) {
                    value.Parent = this;
                }

                metaClass = value;
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

        public ClassDefinition(
            NameExpression name,
            BlockStatement block,
            TypeName[]     bases     = null,
            Expression[]   keywords  = null,
            Expression     metaClass = null
        ) {
            Name      = name;
            Block     = block;
            Bases     = bases ?? new TypeName[0];
            Keywords  = keywords ?? new Expression[0];
            MetaClass = metaClass;
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + "class " + Name + " " + Block;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + "class " + Name + " " + Block;
        }
    }
}