using System.Collections.Generic;
using Axion.Core.Processing.Syntactic.Expressions;

namespace Axion.Core.Processing.Syntactic.Statements.Interfaces {
    public interface IDecorated {
        List<Expression> Modifiers { get; set; }
    }
}