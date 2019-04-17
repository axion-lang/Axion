using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def:
    ///             'class' name ['(' args_list ')'] block
    ///     </c>
    /// </summary>
    public class ClassDefinition : Statement, IDecorated {
        #region Properties

        private NameExpression name;

        public NameExpression Name {
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

        #endregion

        public ClassDefinition(
            NameExpression        name,
            BlockStatement        block,
            NodeList<TypeName>?   bases     = null,
            NodeList<Expression>? keywords  = null,
            Expression?           metaClass = null
        ) {
            Name      = name;
            Block     = block;
            Bases     = bases ?? new NodeList<TypeName>(this);
            Keywords  = keywords ?? new NodeList<Expression>(this);
            MetaClass = metaClass;
        }

        internal ClassDefinition(SyntaxTreeNode parent) : base(parent) {
            Bases    = new NodeList<TypeName>(this);
            Keywords = new NodeList<Expression>(this);
            MarkStart(TokenType.KeywordClass);

            Name = new NameExpression(this, true);
            // TODO: add generic classes
            MetaClass = null;
            List<(TypeName?, NameExpression?)> types = TypeName.ParseNamedTypeArgs(this);
            foreach ((TypeName? type, NameExpression? typeLabel) in types) {
                if (typeLabel == null) {
                    Bases.Add(type);
                }
                else {
                    Keywords.Add(type);
                    if (typeLabel.Qualifiers.Count == 1
                        && typeLabel.Qualifiers[0] == "metaclass") {
                        MetaClass = type;
                    }
                }
            }

            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
        }

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("class ", Name, " ", Block);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("public class ", Name, " ", Block);
        }

        #endregion
    }
}