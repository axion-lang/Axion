using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Traversal;

public abstract class Reducer<T> : INodeReducer where T : Node {
    public void Accept(Ast ast) {
        ast.Traverse<T>(Reduce);
    }

    protected abstract void Reduce(T node);
}
