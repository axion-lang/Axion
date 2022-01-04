using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Traversal;

public interface INodeReducer {
    void Accept(Ast ast);
}
