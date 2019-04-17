using Axion.Core.Processing.CodeGen;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
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

        /// <summary>
        ///     Constructs new <see cref="SimpleTypeName"/> from Axion tokens.
        /// </summary>
        public SimpleTypeName(NameExpression name) {
            Name = name;
            MarkPosition(Name);
        }

        public SimpleTypeName(string name) {
            Name = new NameExpression(null, name);
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Name);
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write(Name);
        }
    }
}