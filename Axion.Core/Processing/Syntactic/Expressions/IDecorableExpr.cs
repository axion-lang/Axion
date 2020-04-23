namespace Axion.Core.Processing.Syntactic.Expressions {
    public interface IDecorableExpr {
        public DecoratedExpr WithDecorators(params Expr[] items);
    }
}
