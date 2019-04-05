using System.Collections.Generic;
using System.Linq;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         enum_def ::=
    ///             'enum' name ['(' args_list ')']
    ///             block_start enum_item* block_terminator
    ///     </c>
    /// </summary>
    public class EnumDefinition : Statement, IDecorated {
        private Expression name;

        [NotNull]
        public Expression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> bases;

        public NodeList<TypeName> Bases {
            get => bases;
            set => SetNode(ref bases, value);
        }

        private NodeList<EnumItem> items;

        public NodeList<EnumItem> Items {
            get => items;
            set => SetNode(ref items, value);
        }

        public List<Expression> Modifiers { get; set; }

        public EnumDefinition(
            [NotNull] SyntaxTreeNode parent,
            [NotNull] Expression     name,
            NodeList<TypeName>       bases = null,
            NodeList<EnumItem>       items = null
        ) {
            Parent = parent;
            Name   = name;
            Bases  = bases;
            Items  = items;
        }

        internal EnumDefinition(SyntaxTreeNode parent) {
            Parent = parent;
            Items  = new NodeList<EnumItem>(this);
            StartNode(TokenType.KeywordEnum);

            Name = new NameExpression(this, true);

            // TODO: support for functions in enums.
            Bases = new NodeList<TypeName>(
                parent,
                TypeName.ParseTypeArgs(this, false).Select(it => it.Item2)
            );
            (TokenType terminator, _, bool error) = BlockStatement.ParseStart(this);

            if (!MaybeEat(terminator) && !MaybeEat(TokenType.KeywordPass) && !error) {
                do {
                    MaybeEatNewline();
                    Items.Add(new EnumItem(this));
                } while (!MaybeEat(terminator)
                         && !CheckUnexpectedEoc()
                         && MaybeEat(TokenType.Comma));
            }

            MarkEnd(Token);
        }
    }

    /// <summary>
    ///     <c>
    ///         enum_item ::=
    ///             name ['(' [type {',' type}] ')'] ['=' constant_expr]
    ///     </c>
    /// </summary>
    public class EnumItem : Expression {
        private NameExpression name;

        [NotNull]
        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> typeList;

        [NotNull]
        public NodeList<TypeName> TypeList {
            get => typeList;
            set => SetNode(ref typeList, value);
        }

        private ConstantExpression val;

        public ConstantExpression Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public EnumItem(
            [NotNull] SyntaxTreeNode parent,
            [NotNull] NameExpression name,
            NodeList<TypeName>       typeList = null,
            ConstantExpression       value    = null
        ) {
            Parent   = parent;
            Name     = name;
            TypeList = typeList ?? new NodeList<TypeName>(this);
            Value    = value;
            MarkStart(Name);
            MarkEnd(
                Value
                ?? (TypeList.Count > 0 ? (SyntaxTreeNode) TypeList.Last : Name)
            );
        }

        internal EnumItem(SyntaxTreeNode parent) {
            Parent = parent;
            MarkStart(Token);

            Name = new NameExpression(this, true);
            TypeList = new NodeList<TypeName>(
                this,
                TypeName.ParseTypeArgs(this, false).Select(it => it.Item2).ToList()
            );
            if (MaybeEat(TokenType.OpAssign)) {
                Value = ParsePrimary(this) as ConstantExpression;
                if (Value == null) {
                    Unit.Blame(BlameType.ConstantValueExpected, Token);
                }
            }

            MarkEnd(Token);
        }
    }
}