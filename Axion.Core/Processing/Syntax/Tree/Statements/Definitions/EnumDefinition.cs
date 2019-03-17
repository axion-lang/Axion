using System.Collections.Generic;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class EnumDefinition : Statement, IDecorated {
        private Expression name;

        public Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private TypeName[] bases;

        public TypeName[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (TypeName expr in bases) {
                    expr.Parent = this;
                }
            }
        }

        private EnumItem[] items;

        public EnumItem[] Items {
            get => items;
            set {
                items = value;
                foreach (EnumItem expr in items) {
                    expr.Parent = this;
                }
            }
        }

        public bool             IsEmpty   => Items.Length == 0;
        public List<Expression> Modifiers { get; set; }

        public EnumDefinition(
            Token      start,
            Token      end,
            Expression name,
            TypeName[] bases = null,
            EnumItem[] items = null
        ) {
            Name  = name;
            Bases = bases ?? new TypeName[0];
            Items = items ?? new EnumItem[0];
            MarkPosition(start, end);
        }
    }

    public class EnumItem : SyntaxTreeNode {
        public NameExpression     Name     { get; }
        public TypeName[]         TypeList { get; }
        public ConstantExpression Value    { get; }

        public EnumItem(NameExpression name, TypeName[] typeList, ConstantExpression value = null) {
            Name     = name;
            TypeList = typeList;
            Value    = value;
            MarkStart(Name);
            MarkEnd(
                Value
                ?? (typeList.Length > 0 ? (SyntaxTreeNode) typeList[typeList.Length - 1] : Name)
            );
        }
    }
}