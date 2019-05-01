using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple_type:
    ///             name;
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName {
        private NameExpression name;

        public NameExpression Name {
            get => name;
            set => SetNode(ref name, value);
        }

        /// <summary>
        ///     Constructs expression from Axion tokens.
        /// </summary>
        public SimpleTypeName(NameExpression name) {
            Name = name;
            MarkPosition(Name);
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