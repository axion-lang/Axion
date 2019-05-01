using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def:
    ///             'class' name ['(' type_arg_list ')'] block
    ///     </c>
    /// </summary>
    public class ClassDefinition : Statement, IDecorated {
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

        private Expression? metaClass;

        public Expression? MetaClass {
            get => metaClass;
            set => SetNode(ref metaClass, value);
        }

        private BlockStatement block;

        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        private NodeList<Expression> modifiers;

        public NodeList<Expression> Modifiers {
            get => modifiers ??= new NodeList<Expression>(this);
            set {
                if (value != null) {
                    modifiers = value;
                }
            }
        }

        /// <summary>
        ///     Constructs from tokens.
        /// </summary>
        internal ClassDefinition(SyntaxTreeNode parent) : base(parent) {
            Bases    = new NodeList<TypeName>(this);
            Keywords = new NodeList<Expression>(this);
            // TODO: pushclass => popclass & add to ready classes.
            ParentBlock.Classes.Add(this);
            EatStartMark(KeywordClass);

            Name = new SimpleNameExpression(this);
            // TODO: add generic classes
            MetaClass = null;
            if (MaybeEat(OpenParenthesis)) {
                List<(TypeName, SimpleNameExpression?)> types = TypeName.ParseNamedTypeArgs(this);
                Eat(CloseParenthesis);
                foreach ((TypeName type, SimpleNameExpression? typeLabel) in types) {
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

            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
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