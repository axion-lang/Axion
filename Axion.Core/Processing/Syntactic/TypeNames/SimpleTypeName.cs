using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Atomic;

namespace Axion.Core.Processing.Syntactic.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple_type:
    ///             name
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        public SimpleTypeName(Expression parent) {
            Construct(parent, () => { Name = NameExpression.ParseName(this); });
        }

        public SimpleTypeName(string name) {
            Name = NameExpression.ParseName(this, name);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Name);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Name);
        }
    }
}