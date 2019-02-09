namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class ArrayTypeName : TypeName {
        public readonly TypeName Target;

        public ArrayTypeName(TypeName target) {
            Target = target;
        }
    }
}