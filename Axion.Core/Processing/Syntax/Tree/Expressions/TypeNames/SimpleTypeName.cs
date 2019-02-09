namespace Axion.Core.Processing.Syntax.Tree.Expressions.TypeNames {
    public class SimpleTypeName : TypeName {
        public readonly NameExpression Name;

        public SimpleTypeName(NameExpression name) {
            Name = name;
        }
    }
}