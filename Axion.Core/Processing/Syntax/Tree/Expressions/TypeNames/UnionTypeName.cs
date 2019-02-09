namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class UnionTypeName : TypeName {
        public readonly TypeName Left;
        public readonly TypeName Right;

        public UnionTypeName(TypeName left, TypeName right) {
            Left  = left;
            Right = right;
        }
    }
}