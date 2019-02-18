using Axion.Core.Specification;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class CallExpression : Expression {
        private Expression target;

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        private Arg[] args;

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

        internal override string CannotAssignReason => Spec.ERR_InvalidAssignmentTarget;

        public CallExpression(Expression target, Arg[] args, Position end) {
            Target = target;
            Args   = args ?? new Arg[0];

            MarkStart(target);
            MarkEnd(end);
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

    public class Arg : SyntaxTreeNode {
        internal NameExpression Name  { get; }
        internal Expression     Value { get; }

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

        public override string ToString() {
            return ToAxionCode();
        }

        internal ArgumentKind GetArgumentInfo() {
            if (Name == null) {
                return ArgumentKind.Simple;
            }
            if (Name.Name.Value == "*") {
                return ArgumentKind.List;
            }
            if (Name.Name.Value == "**") {
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