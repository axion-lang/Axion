using System.Collections.Generic;

namespace Axion.Tokens
{
    internal class FunctionCallToken : Token
    {
        internal readonly Token FunctionToken;
        internal readonly List<Token> ArgumentTokens;

        internal FunctionCallToken(Token functionToken, List<Token> argumentTokens)
        {
            FunctionToken = functionToken;
            ArgumentTokens = argumentTokens;
        }

        public override string ToString()
        {
            var argsString = string.Join(",\r\n    ", ArgumentTokens?.ToString() ?? "Unknown");
            return $"[Call,\r\n    {FunctionToken?.ToString() ?? "Unknown"},\r\n    {argsString}\r\n],";
        }
    }
}
