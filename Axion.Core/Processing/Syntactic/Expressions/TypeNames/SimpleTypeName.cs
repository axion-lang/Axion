using Axion.Core.Processing.CodeGen;
using JetBrains.Annotations;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple_type ::=
    ///             name
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName {
        private Expression name;

        public Expression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        public SimpleTypeName([NotNull] string name) {
            Name = new NameExpression(name);
        }

        public SimpleTypeName([NotNull] Expression name) {
            Name = name;
            MarkPosition(Name);
        }

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            return c + Name;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            return c + Name;
        }
    }
}