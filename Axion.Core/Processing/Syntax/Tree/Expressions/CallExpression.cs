using System;
using Axion.Core.Processing.Lexical.Tokens;
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

        public CallExpression(Expression target, Arg[] args) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Args   = args ?? new Arg[0];
        }

        public CallExpression(Expression target, Arg[] args, Token end) : this(target, args) {
            MarkPosition(target, end);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            c = c + Target + "(";
            c.AppendJoin(",", Args);
            return c + ")";
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            c = c + Target + "(";
            c.AppendJoin(",", Args);
            return c + ")";
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

            MarkPosition(name, value);
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

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            if (Name != null) {
                c = c + Name + " = ";
            }

            return c + Value;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            if (Name != null) {
                c = c + Name + " = ";
            }

            return c + Value;
        }
    }
}