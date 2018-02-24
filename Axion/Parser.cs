using System.Collections.Generic;
using System.IO;
using System.Linq;
using Axion.Tokens;

namespace Axion
{
    internal class Parser
    {
        private readonly TokenType[] EndTokenTypes;
        private readonly List<Token> Tokens;
        public readonly List<Token> SyntaxTree = new List<Token>();
        private int TokenIndex;

        internal Parser(List<Token> tokens, params TokenType[] endTokenTypes)
        {
            Tokens = tokens;
            EndTokenTypes = endTokenTypes;
        }

        internal void Parse()
        {
            for (; TokenIndex < Tokens.Count; TokenIndex++)
            {
                var expression = NextExpression(null);
                if (expression != null)
                {
                    SyntaxTree.Add(expression);
                }
            }
        }

        private Token NextExpression(Token previousToken)
        {
            if (TokenIndex >= Tokens.Count)
            {
                return previousToken;
            }

            var token = Tokens[TokenIndex];
            var type = token.Type;

            if (EndTokenTypes.Contains(type))
            {
                return previousToken;
            }

            TokenIndex++;
            
            if (previousToken is null && (type.ToString("G").ToLower().StartsWith("number") ||
                                          type == TokenType.String ||
                                          type == TokenType.Identifier))
            {
                return NextExpression(token);
            }
            // OPERATION
            if (type == TokenType.Operator)
            {
                var nextToken = NextExpression(null);
                return NextExpression(new OperationToken(token.Value, previousToken, nextToken));
            }
            // FUNCTION CALL
            if (type == TokenType.OpenParenthese && previousToken?.Type == TokenType.Identifier)
            {
                var arguments = MultipleExpressions(TokenType.Comma, TokenType.CloseParenthese);
                return NextExpression(new FunctionCallToken(previousToken, arguments));
            }
            
			Program.LogError("ERROR: Unknown token type: " + type.ToString("G"));
            return NextExpression(token);
        }

        private List<Token> MultipleExpressions(TokenType separatorType, TokenType endTokenType)
        {
            var ret = new List<Token>();
            var type = Tokens[TokenIndex].Type;
            if (type == endTokenType)
            {
                TokenIndex++;
            }
            else
            {
                var parser = new Parser(Tokens.GetRange(0, TokenIndex), separatorType, endTokenType);
                while (type != endTokenType)
                {
                    var token = parser.NextExpression(null);
                    if (token != null)
                    {
                        ret.Add(token);
                    }
                    else break;
                    type = token.Type;
                    TokenIndex++;
                }
            }
            return ret;
        }

        public void SaveFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            
            File.WriteAllLines(fileName, SyntaxTree.Select(token => token?.ToString()));
        }
    }
}
