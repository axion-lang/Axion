using Axion.Processing;

namespace Axion.Tokens.Ast {
    public class Ast {
        internal readonly SourceCode Source;

        internal Ast(SourceCode sourceCode) {
            Source = sourceCode;
        }
    }
}