using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class EnumDefinition : Statement, IDecorated {
        private Expression name;

        private TypeName[] bases;

        private EnumItem[] items;

        public EnumDefinition(
            Position       start,
            Position       end,
            NameExpression name,
            TypeName[]     bases = null,
            EnumItem[]     items = null
        ) {
            Name  = name;
            Bases = bases ?? new TypeName[0];
            Items = items ?? new EnumItem[0];
            MarkPosition(start, end);
        }

        public bool IsEmpty => Items.Length == 0;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        [JsonProperty]
        internal TypeName[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (TypeName expr in bases) {
                    expr.Parent = this;
                }
            }
        }

        [JsonProperty]
        internal EnumItem[] Items {
            get => items;
            set {
                items = value;
                foreach (EnumItem expr in items) {
                    expr.Parent = this;
                }
            }
        }

        [JsonProperty]
        public List<Expression> Modifiers { get; set; }
    }

    public class EnumItem : TreeNode {
        public EnumItem(NameExpression name, TypeName[] typeList, ConstantExpression value = null) {
            Name     = name;
            TypeList = typeList;
            Value    = value;
            MarkStart(Name);
            MarkEnd(Value ?? (typeList.Length > 0 ? (SpannedRegion) typeList[typeList.Length - 1] : Name));
        }

        public NameExpression     Name     { get; }
        public TypeName[]         TypeList { get; }
        public ConstantExpression Value    { get; }
    }
}