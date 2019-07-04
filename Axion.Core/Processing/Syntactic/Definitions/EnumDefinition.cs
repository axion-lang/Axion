using System.Linq;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Atomic;
using Axion.Core.Processing.Syntactic.Interfaces;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;
using static Axion.Core.Specification.TokenType;

namespace Axion.Core.Processing.Syntactic.Definitions {
    /// <summary>
    ///     <c>
    ///         enum_def:
    ///             'enum' name ['(' type_arg_list ')']
    ///             block_start enum_item {',' enum_item} block_terminator
    ///     </c>
    /// </summary>
    public class EnumDefinition : Expression, IDecorable {
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
            get => modifiers;
            set => SetNode(ref modifiers, value);
        }

        internal EnumDefinition(Expression parent) {
            Construct(parent, () => {
                Items = new NodeList<EnumItem>(this);
                Eat(KeywordEnum);

                Name = new SimpleNameExpression(this);
                // TODO: support for functions in enums.
                if (MaybeEat(OpenParenthesis)) {
                    Bases = new NodeList<TypeName>(
                        this,
                        TypeName.ParseNamedTypeArgs(this).Select(a => a.type)
                    );
                    Eat(CloseParenthesis);
                }
                else {
                    Bases = new NodeList<TypeName>(this);
                }

                ParentBlock.RegisterNamedNode(
                    this,
                    () => {
                        (TokenType terminator, bool error) = BlockExpression.ParseStart(this);
                        if (MaybeEat(terminator, KeywordPass) || error) {
                            return;
                        }

                        do {
                            MaybeEat(Newline);
                            Items.Add(new EnumItem(this));
                        } while (!MaybeEat(terminator)
                              && MaybeEat(Comma));
                    }
                );
            });
        }

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
            bool haveAccessMod = c.WriteDecorators(Modifiers);
            if (!haveAccessMod) {
                c.Write("public ");
            }

            c.Write("enum ", Name);
            if (Bases.Count > 1) {
                Unit.ReportError("C# enum cannot be inherited from more than 1 type.", Bases[1]);
            }

            c.Write(" {");
            c.AddJoin(",", Items);
            c.Write("}");
        }
    }

    /// <summary>
    ///     <c>
    ///         enum_item:
    ///         name ['(' [type {',' type}] ')'] ['=' constant_expr]
    ///     </c>
    /// </summary>
    public class EnumItem : Expression {
        private SimpleNameExpression name;

        public SimpleNameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        private NodeList<TypeName> typeList;

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
            Expression           parent,
            SimpleNameExpression name,
            NodeList<TypeName>   typeList = null,
            ConstantExpression   value    = null
        ) : base(parent) {
            Name     = name;
            TypeList = typeList ?? new NodeList<TypeName>(this);
            Value    = value;
            MarkStart(Name);
            MarkEnd(
                Value
             ?? (TypeList.Count > 0 ? (Expression) TypeList.Last : Name)
            );
        }

        internal EnumItem(Expression parent) : base(parent) {
            MarkStart();

            Name = new SimpleNameExpression(this);
            if (MaybeEat(OpenParenthesis)) {
                TypeList = new NodeList<TypeName>(
                    this,
                    TypeName.ParseNamedTypeArgs(this).Select(a => a.type)
                );
                Eat(CloseParenthesis);
            }
            else {
                TypeList = new NodeList<TypeName>(this);
            }

            if (MaybeEat(OpAssign)) {
                Value = ParseAtomExpr(this) as ConstantExpression;
                if (Value == null) {
                    Unit.Blame(BlameType.ConstantValueExpected, Token);
                }
            }

            MarkEnd();
        }

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
    }
}