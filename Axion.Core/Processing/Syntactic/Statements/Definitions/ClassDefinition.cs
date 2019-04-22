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

        #endregion

        internal ClassDefinition(SyntaxTreeNode parent) : base(parent) {
            Bases    = new NodeList<TypeName>(this);
            Keywords = new NodeList<Expression>(this);
            MarkStart(TokenType.KeywordClass);

            Name = new SimpleNameExpression(this);
            // TODO: add generic classes
            MetaClass = null;
            List<(TypeName?, SimpleNameExpression?)> types = TypeName.ParseNamedTypeArgs(this);
            foreach ((TypeName? type, SimpleNameExpression? typeLabel) in types) {
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

            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
            ParentBlock.Classes.Add(this);
        }

        #region Code converters

        public override void ToAxionCode(CodeBuilder c) {
            c.Write("class ", Name, " ", Block);
        }

        public override void ToCSharpCode(CodeBuilder c) {
            c.Write("public class ", Name, " ", Block);
        }

        #endregion
    }
}