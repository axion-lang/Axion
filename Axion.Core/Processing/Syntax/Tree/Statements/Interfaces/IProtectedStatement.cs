using Axion.Core.Processing.Lexical.Tokens;

namespace Axion.Core.Processing.Syntax.Tree.Statements.Interfaces {
    public interface IProtectedStatement {
        TokenType ProtectionLevel { get; }
    }
}