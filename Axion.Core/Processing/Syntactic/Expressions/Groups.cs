namespace Axion.Core.Processing.Syntactic.Expressions {
    public interface IDecoratedExpr { }

    public interface IStatementExpr { }

    public interface IDefinitionExpr {
        Expr Name { get; set; }
    }

    public interface IGlobalExpr { }

    public interface IInfixExpr : IGlobalExpr { }

    public interface IAtomExpr : IInfixExpr { }

    public interface IAssignableExpr : IAtomExpr { }

    public interface IVarTargetExpr : IAssignableExpr { }
}