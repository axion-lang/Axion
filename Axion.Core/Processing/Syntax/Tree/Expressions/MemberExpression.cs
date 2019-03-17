using System;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Syntax.Tree.Expressions {
    public class MemberExpression : Expression {
        private Expression target;

        [JsonProperty]
        internal Expression Target {
            get => target;
            set {
                value.Parent = this;
                target       = value;
            }
        }

        private NameExpression member;

        [JsonProperty]
        internal NameExpression Member {
            get => member;
            set {
                value.Parent = this;
                member       = value;
            }
        }

        internal override string CannotAssignReason =>
            Target.CannotAssignReason ?? Member.CannotAssignReason;

        public MemberExpression(Expression target, NameExpression member) {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Member = member ?? throw new ArgumentNullException(nameof(member));

            MarkPosition(target, member);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Target + "." + Member;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Target + "." + Member;
        }
    }
}