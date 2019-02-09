namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class TupleTypeName : TypeName {
        public readonly TypeName[] Types;

        public TupleTypeName(TypeName[] types) {
            Types = types;
        }
    }
}