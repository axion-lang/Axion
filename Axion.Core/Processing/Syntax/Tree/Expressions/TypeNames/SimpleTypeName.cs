using System;

namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class SimpleTypeName : TypeName {
        public readonly Expression Name;

        public SimpleTypeName(string name) {
            Name = new NameExpression(name);
        }

        public SimpleTypeName(Expression name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MarkPosition(Name);
        }

        internal override AxionCodeBuilder ToAxionCode(AxionCodeBuilder c) {
            return c + Name;
        }

        internal override CSharpCodeBuilder ToCSharpCode(CSharpCodeBuilder c) {
            return c + Name;
        }
    }
}