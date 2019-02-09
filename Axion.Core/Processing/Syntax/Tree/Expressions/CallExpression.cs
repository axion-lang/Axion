using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class CallExpression : Expression {
        private Expression target;

        private Arg[] args;

        public CallExpression(Expression target, Arg[] args, Position end) {
            Target = target;
            Args   = args ?? new Arg[0];

            MarkStart(target);
            MarkEnd(end);
        }

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        [JsonProperty]
        internal Arg[] Args {
            get => args;
            set {
                args = value;
                foreach (Arg arg in args) {
                    arg.Parent = this;
                }
            }
        }

        public override string ToString() {
            return ToAxionCode();
        }

        private string ToAxionCode() {
            return Target + "(" + string.Join<Arg>(",", Args) + ")";
        }
    }

    public enum ArgumentKind {
        Simple,
        Named,
        List,
        Map
    }

    public class Arg : TreeNode {
        internal Arg(Expression value) {
            Value = value;

            MarkPosition(value);
        }

        internal Arg(NameExpression name, Expression value) {
            Name  = name;
            Value = value;

            MarkStart(name);
            MarkEnd(value);
        }

        internal NameExpression Name  { get; }
        internal Expression     Value { get; }

        public override string ToString() {
            return ToAxionCode();
        }

        internal ArgumentKind GetArgumentInfo() {
            if (Name == null) {
                return ArgumentKind.Simple;
            }
            if (Name.Name == "*") {
                return ArgumentKind.List;
            }
            if (Name.Name == "**") {
                return ArgumentKind.Map;
            }
            return ArgumentKind.Named;
        }

        private string ToAxionCode() {
            var name = "";
            if (Name != null) {
                name = Name.Name + " = ";
            }
            return name + Value;
        }
    }
}