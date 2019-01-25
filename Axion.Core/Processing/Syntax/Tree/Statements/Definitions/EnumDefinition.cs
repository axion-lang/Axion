using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;
using Axion.Core.Processing.Syntax.Tree.Statements.Interfaces;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Definitions {
    public class EnumDefinition : Statement, IDecorated {
        private Expression name;

        [JsonProperty]
        internal Expression Name {
            get => name;
            set {
                value.Parent = this;
                name         = value;
            }
        }

        private Expression[] bases;

        [JsonProperty]
        internal Expression[] Bases {
            get => bases;
            set {
                bases = value;
                foreach (Expression expr in bases) {
                    expr.Parent = this;
                }
            }
        }

        private EnumItem[] items;

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

        public bool IsEmpty => Items.Length == 0;

        [JsonProperty]
        public List<Expression> Modifiers { get; set; }

        public EnumDefinition(Position start, Position end, NameExpression name, Expression[] bases = null, EnumItem[] items = null) {
            Name  = name;
            Bases = bases ?? new Expression[0];
            Items = items ?? new EnumItem[0];
            MarkPosition(start, end);
        }
    }

    public class EnumItem : TreeNode {
        public NameExpression     Name     { get; }
        public NameExpression[]   TypeList { get; }
        public ConstantExpression Value    { get; }

        public EnumItem(NameExpression name, NameExpression[] typeList, ConstantExpression value = null) {
            Name     = name;
            TypeList = typeList;
            Value    = value;
            MarkStart(Name);
            MarkEnd((SpannedRegion) Value ?? (typeList.Length > 0 ? typeList[typeList.Length - 1] : Name));
        }
    }
}