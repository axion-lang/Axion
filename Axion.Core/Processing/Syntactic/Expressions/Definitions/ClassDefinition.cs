using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;
using Axion.Core.Processing.Syntactic.Expressions.Interfaces;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Expressions.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def:
    ///             'class' name ['&lt;' type_arg_list] block
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

        private Expression metaClass;

        public Expression MetaClass {
            get => metaClass;
            set => SetNode(ref metaClass, value);
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
        internal ClassDefinition(AstNode parent) : base(parent) {
            Bases    = new NodeList<TypeName>(this);
            Keywords = new NodeList<Expression>(this);
            MarkStartAndEat(TokenType.KeywordClass);

            Name = new SimpleNameExpression(this);
            // TODO: add generic classes, case classes
            if (MaybeEat(TokenType.OpLess)) {
                List<(TypeName, SimpleNameExpression )> types = TypeName.ParseNamedTypeArgs(this);
                foreach ((TypeName type, SimpleNameExpression typeLabel) in types) {
                    if (typeLabel == null) {
                        Bases.Add(type);
                    }
                    else {
                        Keywords.Add(type);
                        if (typeLabel.Name == "metaclass") {
                            MetaClass = type;
                        }
                    }
                }
            }

            Block = new BlockExpression(this, BlockType.Named);

            MarkEnd();
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