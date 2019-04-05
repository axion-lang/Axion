using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         class_def ::=
    ///             'class' name ['(' args_list ')'] block
    ///     </c>
    /// </summary>
    public class ClassDefinition : Statement, IDecorated {
        private NameExpression name;

        [NotNull]
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

        private Expression metaClass;

        public Expression MetaClass {
            get => metaClass;
            set => SetNode(ref metaClass, value);
        }

        private BlockStatement block;

        [NotNull]
        public BlockStatement Block {
            get => block;
            set => SetNode(ref block, value);
        }

        public List<Expression> Modifiers { get; set; }

        public ClassDefinition(
            [NotNull] NameExpression name,
            [NotNull] BlockStatement block,
            NodeList<TypeName>       bases     = null,
            NodeList<Expression>     keywords  = null,
            Expression               metaClass = null
        ) {
            Name      = name;
            Block     = block;
            Bases     = bases;
            Keywords  = keywords;
            MetaClass = metaClass;
        }

        internal ClassDefinition(SyntaxTreeNode parent) {
            Parent   = parent;
            Bases    = new NodeList<TypeName>(this);
            Keywords = new NodeList<Expression>(this);
            StartNode(TokenType.KeywordClass);

            Name = new NameExpression(this, true);
            // TODO: add generic classes
            MetaClass = null;
            List<(NameExpression, TypeName)> types = TypeName.ParseTypeArgs(this, true);
            foreach ((NameExpression typeLabel, TypeName type) in types) {
                if (typeLabel == null) {
                    Bases.Add(type);
                }
                else {
                    Keywords.Add(type);
                    if (typeLabel.Qualifiers.Count == 1
                        && typeLabel.Qualifiers[0].Value == "metaclass") {
                        MetaClass = type;
                    }
                }
            }

            Block = new BlockStatement(this, BlockType.Top);

            MarkEnd(Token);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + "class " + Name + " " + Block;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + "class " + Name + Block;
        }
    }
}