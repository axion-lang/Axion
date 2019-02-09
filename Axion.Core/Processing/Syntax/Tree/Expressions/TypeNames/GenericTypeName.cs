namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class GenericTypeName : TypeName {
        public readonly TypeName   Target;
        public readonly TypeName[] Generics;

        public GenericTypeName(TypeName target, TypeName[] generics) {
            Target   = target;
            Generics = generics;
        }
    }
}