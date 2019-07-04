using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Interfaces;
using Axion.Core.Processing.Syntactic.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Definitions {
    public class ObjectDefinition : Expression, IDecorable {
        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> bases;

        public NodeList<TypeName> Bases {
            get => bases;
            set => SetNode(ref bases, value);
        }

        private BlockExpression block;

        public BlockExpression Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expression> modifiers;

        public NodeList<Expression> Modifiers {
            get => modifiers;
            set => SetNode(ref modifiers, value);
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ObjectDefinition(Expression parent) : base(parent) {
            Construct(parent, () => {
                Bases = new NodeList<TypeName>(this);
                Eat(KeywordObject);

                Name = new SimpleNameExpression(this);
                // TODO: add generic classes, case classes
                if (MaybeEat(OpLess)) {
                    List<(TypeName, SimpleNameExpression)> types = TypeName.ParseNamedTypeArgs(this);
                    foreach ((TypeName type, SimpleNameExpression typeLabel) in types) {
                        if (typeLabel == null) {
                            Bases.Add(type);
                        }
                    }
                }

                Block = new BlockExpression(this, BlockType.Named);
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("object ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            bool haveAccessMod = c.WriteDecorators(Modifiers);
            if (!haveAccessMod) {
                c.Write("public ");
            }

            c.Write("class ", Name, " ", Block);
        }
    }
}