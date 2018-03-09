using System;
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

			// Number, String, Identifier, Built-in type
			if ((type.ToString("G").ToLower().StartsWith("number") ||
				 type == TokenType.String ||
				 type == TokenType.Identifier ||
				 type == TokenType.BuiltInType) &&
				previousToken is null)
			{
				return NextExpression(token);
			}

			// Collection indexer - []
			if (type == TokenType.OpenBracket)
			{
				switch (previousToken.Type)
				{
					case TokenType.String:
					case TokenType.Identifier:
						{
							return NextExpression(new IndexerToken(previousToken, Tokens[TokenIndex]));
						}
					default:
						{
							Program.LogError($"Indexer cannot be applied to type : {previousToken.Type:G}", ErrorOrigin.Parser);
							return null;
						}
				}
			}

			// Collections + items
			if (type == TokenType.OpenCurly)
			{
				if (previousToken == null)
				{
					Program.LogError("Collection initializer without identifier. At line 1, column 1", ErrorOrigin.Parser);
					return null;
				}

				Token itemType;
				CollectionType collectionType;

				// array - int{ 1, 2, 3 }
				if (previousToken.Type == TokenType.BuiltInType)
				{
					itemType = previousToken;
					collectionType = CollectionType.Array;
				}
				// list - int*{ 1, 2, 3 }
				else if (previousToken.Value == "*" && Tokens.Count > 3)
				{
					itemType = Tokens[TokenIndex - 3];
					collectionType = CollectionType.List;
				}
				// 'array() {}', 'list() {}', 'matrix() {}', etc.
				else if (!(previousToken is FunctionCallToken collectionInitCallToken &&
						   Enum.TryParse(collectionInitCallToken.NameToken.Value, true, out collectionType)))
				{
					Program.LogError($"'{{' is at invalid position.\r\nDebug Info:\r\n{previousToken}\r\n{token}", ErrorOrigin.Parser);
					return null;
				}
				else
				{
					itemType = collectionInitCallToken;
				}

				var items = GetTokensList(TokenType.Comma, TokenType.CloseCurly);

				return NextExpression(new CollectionToken(itemType, collectionType, items));
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

			Program.LogError("Invalid position of token: " + type.ToString("G"), ErrorOrigin.Parser, false);
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
