using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Interfaces;
using Axion.Core.Processing.Syntactic.TypeNames;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def:
    ///             'class' simple_name [type_args] ['&lt;' type_arg_list] block;
    ///     </c>
    /// </summary>
    public class ClassDefinition : Expression, IDecorable {
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

        private NodeList<Expression> keywords;

        public NodeList<Expression> Keywords {
            get => keywords;
            set => SetNode(ref keywords, value);
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
        ///     Expression is constructed from tokens stream
        ///     that belongs to <see cref="parent"/>'s AST.
        /// </summary>
        internal ClassDefinition(Expression parent) {
            Construct(parent, () => {
                Bases    = new NodeList<TypeName>(this);
                Keywords = new NodeList<Expression>(this);
                Eat(KeywordClass);

                Name = new SimpleNameExpression(this);
                // TODO: add generic classes, case classes
                if (MaybeEat(OpLess)) {
                    List<(TypeName, SimpleNameExpression )> types = TypeName.ParseNamedTypeArgs(this);
                    foreach ((TypeName type, SimpleNameExpression typeLabel) in types) {
                        if (typeLabel == null) {
                            Bases.Add(type);
                        }
                        else {
                            Keywords.Add(type);
                        }
                    }
                }

                Block = new BlockExpression(this, BlockType.Named);
            });
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("class ", Name, " ", Block);
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