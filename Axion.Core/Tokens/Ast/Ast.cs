using Axion.Core.Processing;

namespace Axion.Core.Tokens.Ast {
    public class Ast {
        internal readonly SourceCode Source;

        internal Ast(SourceCode sourceCode) {
            Source = sourceCode;
        }
    }
}