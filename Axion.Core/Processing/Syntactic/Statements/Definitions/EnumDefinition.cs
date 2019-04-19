using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Processing.Syntactic.Statements.Interfaces;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Syntactic.Statements.Definitions {
    /// <summary>
    ///     <c>
    ///         enum_def:
    ///             'enum' name ['(' args_list ')']
    ///             block_start enum_item {',' enum_item} block_terminator
    ///     </c>
    /// </summary>
    public class EnumDefinition : Statement, IDecorated {
        #region Properties

        private Expression name;

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

        public EnumDefinition(
            SyntaxTreeNode      parent,
            Expression          name,
            NodeList<TypeName>? bases = null,
            NodeList<EnumItem>? items = null
        ) : base(parent) {
            Name  = name;
            Bases = bases ?? new NodeList<TypeName>(this);
            Items = items ?? new NodeList<EnumItem>(this);
        }

        internal EnumDefinition(SyntaxTreeNode parent) : base(parent) {
            Items = new NodeList<EnumItem>(this);
            MarkStart(TokenType.KeywordEnum);

            Name = new NameExpression(this, true);

            // TODO: support for functions in enums.
            Bases                              = TypeName.ParseTypeArgs(this);
            (TokenType terminator, bool error) = BlockStatement.ParseStart(this);

            if (!MaybeEat(terminator) && !MaybeEat(TokenType.KeywordPass) && !error) {
                do {
                    MaybeEatNewline();
                    Items.Add(new EnumItem(this));
                } while (!MaybeEat(terminator)
                         && MaybeEat(TokenType.Comma));
            }

            MarkEnd(Token);
        }

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write("enum ", Name);
            if (Bases.Count > 0) {
                c.Write(" (");
                c.AddJoin(",", Bases);
                c.Write(")");
            }

            if (Items.Count > 0) {
                c.Write(" {");
                c.AddJoin(",", Items);
                c.Write("}");
            }
            else {
                c.Write(" pass");
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("enum ", Name);
            if (Bases.Count > 1) {
                Unit.ReportError("C# enum cannot be inherited from more than 1 type.", Bases[1]);
            }

            c.Write(" {");
            c.AddJoin(",", Items);
            c.Write("}");
        }

        #endregion
    }

    /// <summary>
    ///     <c>
    ///         enum_item:
    ///             name ['(' [type {',' type}] ')'] ['=' constant_expr]
    ///     </c>
    /// </summary>
    public class EnumItem : Expression {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> typeList;

        public NodeList<TypeName> TypeList {
            get => typeList;
            set => SetNode(ref typeList, value);
        }

        private ConstantExpression? val;

        public ConstantExpression? Value {
            get => val;
            set => SetNode(ref val, value);
        }

        public EnumItem(
            SyntaxTreeNode      parent,
            NameExpression      name,
            NodeList<TypeName>? typeList = null,
            ConstantExpression? value    = null
        ) : base(parent) {
            Name     = name;
            TypeList = typeList ?? new NodeList<TypeName>(this);
            Value    = value;
            MarkStart(Name);
            MarkEnd(
                Value
                ?? (TypeList.Count > 0 ? (SyntaxTreeNode) TypeList.Last : Name)
            );
        }

        internal EnumItem(SyntaxTreeNode parent) : base(parent) {
            MarkStart(Token);

            Name     = new NameExpression(this, true);
            TypeList = TypeName.ParseTypeArgs(this);
            if (MaybeEat(TokenType.OpAssign)) {
                Value = ParsePrimaryExpr(this) as ConstantExpression;
                if (Value == null) {
                    Unit.Blame(BlameType.ConstantValueExpected, Token);
                }
            }

            MarkEnd(Token);
        }

        #region Code converters

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Name);
            if (TypeList.Count > 0) {
                c.Write(" (");
                c.AddJoin(",", TypeList);
                c.Write(")");
            }

            if (Value != null) {
                c.Write(" = ", Value);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Name);
            if (TypeList.Count > 0) {
                Unit.ReportError("C# doesn't support enum items with types.", TypeList[0]);
            }

            if (Value != null) {
                c.Write(" = ", Value);
            }
        }

        #endregion
    }
}