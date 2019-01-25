using System.Collections.Generic;
using Axion.Core.Processing.Syntax.Tree.Expressions;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Interfaces {
    public interface IDecorated {
        List<Expression> Modifiers { get; set; }
    }
}