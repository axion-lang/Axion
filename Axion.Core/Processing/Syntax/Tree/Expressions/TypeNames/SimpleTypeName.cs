namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class SimpleTypeName : TypeName {
        public readonly Expression Name;

        public SimpleTypeName(Expression name) {
            Name = name;
        }
    }
}