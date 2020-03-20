using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.Expressions.Atomic;

namespace Axion.Core.Processing.Syntactic.Expressions.TypeNames {
    /// <summary>
    ///     <c>
    ///         simple-type:
    ///             name;
    ///     </c>
    /// </summary>
    public class SimpleTypeName : TypeName {
        private NameExpr name;

        public NameExpr Name {
            get => name;
            set => SetNode(ref name, value);
        }

        public SimpleTypeName(Expr parent) : base(parent) { }

        public SimpleTypeName Parse() {
            SetSpan(() => { Name = new NameExpr(this).Parse(); });
            return this;
        }

        public SimpleTypeName(string name) {
            Name = new NameExpr(name);
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Name);
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write(Name);
        }

        public override void ToPython(CodeWriter c) {
            c.Write(Name);
        }
    }
}