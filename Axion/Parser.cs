using System.Collections.Generic;
using Axion.Tokens;

namespace Axion
{
	internal static class Parser
	{
		private static readonly List<TokenType> EndTokenTypes = new List<TokenType> { TokenType.Newline };
		private static readonly List<Token> Tokens = Lexer.Tokens;
		public static readonly List<Token> SyntaxTree = new List<Token>();
		private static int TokenIndex;

		internal static void Parse()
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

		private static Token NextExpression(Token previousToken)
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

			// Number, String, Identifier
			if (previousToken is null && (type.ToString("G").ToLower().StartsWith("number") ||
										  type == TokenType.String ||
										  type == TokenType.Identifier ||
			                              type == TokenType.BuiltInType))
			{
				return NextExpression(token);
			}

			// Collections + items
			if (type == TokenType.OpenBracket)
			{
				if (previousToken == null)
				{
					Program.LogError($"Bracket position invalid.\r\nDebug Info: {token}", true);
					return null;
				}

				CollectionType collectionType;

				// array
				if (previousToken.Type == TokenType.BuiltInType)
				{
					collectionType = CollectionType.Array;
				}
				// list
				else if (previousToken.Value == "*")
				{
					collectionType = CollectionType.List;
				}
				else // bracket position invalid then
				{
					Program.LogError($"Bracket position invalid.\r\nDebug Info:\r\n{previousToken}\r\n{token}", true);
					return null;
				}

				var items = GetTokensList(TokenType.Comma, TokenType.CloseBracket);

				return NextExpression(new CollectionToken(previousToken, collectionType, items));
			}

			// Operation TODO add operands on different lines support
			if (type == TokenType.Operator && previousToken.Type != TokenType.BuiltInType)
			{
				// get right operand
				var nextToken = NextExpression(null);
				return NextExpression(new OperationToken(token.Value, previousToken, nextToken));
			}

			// Function call
			if (type == TokenType.OpenParenthese && previousToken?.Type == TokenType.Identifier)
			{
				var arguments = GetTokensList(TokenType.Comma, TokenType.CloseParenthese);
				return NextExpression(new FunctionCallToken(previousToken, arguments));
			}

			Program.LogError("Unknown token type: " + type.ToString("G"));
			return NextExpression(token);
		}

		private static List<Token> GetTokensList(TokenType separatorType, TokenType endTokenType)
		{
			var tokens = new List<Token>();
			var type = Tokens[TokenIndex].Type;
			if (type == endTokenType) // when 'call()'
			{
				TokenIndex++;
			}
			else
			{
				EndTokenTypes.Add(separatorType);
				EndTokenTypes.Add(endTokenType);
				while (type != endTokenType)
				{
					var token = NextExpression(null);
					if (token != null && token.Type != separatorType && token.Type != endTokenType)
					{
						tokens.Add(token);
					}
					type = Tokens[TokenIndex].Type;
					TokenIndex++;
					if (TokenIndex >= Tokens.Count) break;
				}
				EndTokenTypes.Remove(endTokenType);
				EndTokenTypes.Remove(separatorType);
			}
			return tokens;
		}
	}
}
