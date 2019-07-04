using Axion.Core.Processing.Syntactic.Definitions;
using Axion.Core.Processing.Syntactic.TypeNames;

namespace Axion.Core.Processing.Syntactic.Interfaces {
    public interface IFunctionNode {
        NodeList<FunctionParameter> Parameters { get; set; }
        TypeName ReturnType { get; set; }
    }
}