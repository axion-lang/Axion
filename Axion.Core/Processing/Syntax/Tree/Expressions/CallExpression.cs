using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class CallExpression : Expression {
        [JsonProperty]
        internal Expression Target { get; }

        [JsonProperty]
        internal Arg[] Args { get; }

        public CallExpression(Expression target, Arg[] args, Position end) {
            Target = target;
            Args   = args ?? new Arg[0];

            MarkStart(target);
            MarkEnd(end);
        }

        public bool NeedsLocalsMap() {
            if (!(Target is NameExpression nameExpr)) {
                return false;
            }
            if (Args.Length == 0) {
                switch (nameExpr.Name) {
                    case "locals":
                    case "vars":
                    case "dir":
                        return true;
                    default:
                        return false;
                }
            }
            if (Args.Length == 1 && (nameExpr.Name == "dir" || nameExpr.Name == "vars") && (Args[0].Name.Name == "*" || Args[0].Name.Name == "**")
             || Args.Length == 2 && (nameExpr.Name == "dir" || nameExpr.Name == "vars") && Args[0].Name.Name == "*" && Args[1].Name.Name == "**") {
                // could be splatting empty list or map resulting in 0-param call which needs context
                return true;
            }
            return nameExpr.Name == "eval";
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

    public class Arg : SpannedRegion {
        internal NameExpression Name  { get; }
        internal Expression     Value { get; }

        internal Arg(Expression value) {
            Value = value;
            MarkPosition(value);
        }

        internal Arg(NameExpression name, Expression value) {
            Value = value;
            Name  = name;
            MarkStart(name);
            MarkEnd(value);
        }

        internal ArgumentKind GetArgumentInfo() {
            if (Name.Name == null) {
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

        public override string ToString() {
            return ToAxionCode();
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