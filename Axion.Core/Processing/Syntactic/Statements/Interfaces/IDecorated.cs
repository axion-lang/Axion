using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Syntactic.Statements.Interfaces {
    public interface IDecorated {
        NodeList<Expression> Modifiers { get; set; }
    }
}