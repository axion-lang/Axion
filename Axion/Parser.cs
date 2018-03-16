using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Tokens;

namespace Axion
{
	/// <summary>
	///     Creates an expressions syntax tree from tokens list what used in evaluator / compiler.
	/// </summary>
	internal static class Parser
	{
		// expressions usually splitted by new lines or semicolons
		private static readonly List<TokenType> EndTokenTypes = new List<TokenType> { TokenType.Newline, TokenType.EOF };

		/// <summary>
		///     Simplified access to lexer's tokens.
		/// </summary>
		private static readonly List<Token> Tokens = Lexer.Tokens;

		/// <summary>
		///     Resulting expressions syntax tree.
		/// </summary>
		public static readonly List<Token> SyntaxTree = new List<Token>();

		/// <summary>
		///     Current evaluating token's index.
		/// </summary>
		private static int TokenIndex;

		/// <summary>
		///     Makes a tree from lexer's tokens list.
		/// </summary>
		internal static void Parse()
		{
			for (; TokenIndex < Tokens.Count; TokenIndex++)
			{
				Token expression = NextExpression();
				if (expression != null)
				{
					SyntaxTree.Add(expression);
				}
			}
		}

		/// <summary>
		///     Evaluates next expression.
		/// </summary>
		private static Token NextExpression(Token previousToken = null)
		{
			if (TokenIndex >= Tokens.Count)
			{
				return previousToken;
			}

			Token     token = Tokens[TokenIndex];
			TokenType type  = token.Type;

			if (EndTokenTypes.Contains(type))
			{
				return previousToken;
			}

			TokenIndex++;
			Token nextToken = Peek();

			switch (type)
			{
				// Number, String, Identifier, Built-in type
				case TokenType.Identifier when previousToken == null:
				case TokenType.String when previousToken == null:
				case TokenType.Number_Byte when previousToken == null:
				case TokenType.Number_Float when previousToken == null:
				case TokenType.Number_LFloat when previousToken == null:
				case TokenType.Number_Int when previousToken == null:
				case TokenType.Number_SInt when previousToken == null:
				case TokenType.Number_LInt when previousToken == null:
				{
					return NextExpression(token);
				}
				case TokenType.Keyword:
				{
					switch (token.Value)
					{
						case "if":
						{
							List<OperationToken> conditions = ParseIfConditions();
							List<Token>          thenTokens = ParseBlock();
							return NextExpression(new BranchingToken(conditions, thenTokens));
						}
						case "elif" when previousToken is BranchingToken parentBranch:
						{
							List<OperationToken> conditions = ParseIfConditions();
							List<Token>          thenTokens = ParseBlock();
							parentBranch.ElseIfs.Add(conditions, thenTokens);
							return NextExpression(parentBranch);
						}
						case "else" when previousToken is BranchingToken parentBranch:
						{
							// skip 'else:'
							TokenIndex++;
							List<Token> thenTokens = ParseBlock();
							parentBranch.ElseTokens.AddRange(thenTokens);
							return NextExpression(parentBranch);
						}
						default:
						{
							Program.LogError("Unknown keyword: " + token.Value,
							                 ErrorOrigin.Parser, true,
							                 token.LinePosition,
							                 token.ColumnPosition);
							return null;
						}
					}
				}
				// Collection indexer - []
				case TokenType.OpenBracket when previousToken != null:
				{
					switch (previousToken.Type)
					{
						case TokenType.String:
						case TokenType.Identifier:
						{
							Token index = Tokens[TokenIndex];

							// skip number & close bracket
							TokenIndex += 2;
							return NextExpression(new IndexerToken(previousToken, index));
						}
						default:
						{
							Program.LogError($"Indexer cannot be applied to type : {previousToken.Type:G}",
							                 ErrorOrigin.Parser, true,
							                 previousToken.LinePosition,
							                 previousToken.ColumnPosition);
							return null;
						}
					}
				}
				// Collections + items
				case TokenType.OpenCurly when previousToken != null:
				{
					Token          itemType;
					CollectionType collectionType;

					// array - int{ 1, 2, 3 }
					if (previousToken.Type == TokenType.BuiltInType)
					{
						itemType       = previousToken;
						collectionType = CollectionType.Array;
					}
					// list - int*{ 1, 2, 3 }
					else 
					if (previousToken.Value == "*" && // BUG
					         Tokens.Count > 3)
					{
						itemType       = Tokens[TokenIndex - 3];
						collectionType = CollectionType.List;
					}
					// 'array() {}', 'list() {}', 'matrix() {}', etc.
					else 
					if (!(previousToken is FunctionCallToken collectionInitCallToken &&
					           Enum.TryParse(collectionInitCallToken.NameToken.Value, true, out collectionType)))
					{
						Program.LogError("'{{' is at invalid position",
						                 ErrorOrigin.Parser, true,
						                 token.LinePosition,
						                 token.ColumnPosition);
						return null;
					}
					else
					{
						itemType = collectionInitCallToken;
					}

					List<Token> items = GetTokensList(new[] { TokenType.Comma }, new[] { TokenType.CloseCurly });

					return NextExpression(new CollectionToken(itemType, collectionType, items));
				}
				// Operation TODO add operands on different lines support + unary operators + operations priority
				case TokenType.Operator when previousToken?.Type != TokenType.BuiltInType:
				{
					Token rightOperand = NextExpression();
					return NextExpression(new OperationToken(token.Value, previousToken, rightOperand));
				}
				// Function call
				case TokenType.OpenParenthese when previousToken != null && previousToken.Type == TokenType.Identifier:
				{
					List<Token> arguments = GetTokensList(new[] { TokenType.Comma }, new[] { TokenType.CloseParenthese });
					return NextExpression(new FunctionCallToken(previousToken, arguments));
				}
				default:
				{
					Program.LogError("Invalid position of token: " + type.ToString("G"),
					                 ErrorOrigin.Parser, false,
					                 token.LinePosition,
					                 token.ColumnPosition);
					return NextExpression(token);
				}
			}
		}

		private static Token Peek()
		{
			return TokenIndex < Tokens.Count
				? Tokens[TokenIndex]
				: null;
		}

		private static List<Token> ParseBlock()
		{
			List<Token> tokens;
			// Process multiline block (until outdent)
			if (Peek()?.Type == TokenType.Newline &&
			    TokenIndex + 1 < Tokens.Count &&
			    Tokens[TokenIndex + 1].Type == TokenType.Indent)
			{
				TokenIndex += 2;
				tokens     =  GetTokensList(new[] { TokenType.Outdent });
				if (Peek()?.Type == TokenType.Outdent) TokenIndex++;
			}
			// single line block (until first semicolon)
			else
			{
				tokens = GetTokensList(new[] { TokenType.Semicolon });
				if (Peek()?.Type == TokenType.Newline) TokenIndex++;
			}

			return tokens;
		}

		private static List<OperationToken> ParseIfConditions()
		{
			List<Token> conditions = GetTokensList(new[] { TokenType.Colon });

			// if some condition doesn't return a bool
			Token invalidCondition = conditions
				.FirstOrDefault(condition =>
					                !(condition is OperationToken) ||
					                condition.Type != TokenType.Identifier);
			if (invalidCondition != null)
			{
				Program.LogError("Invalid condition",
				                 ErrorOrigin.Parser, true,
				                 invalidCondition.LinePosition,
				                 invalidCondition.ColumnPosition);
			}

			// condition at end of file
			if (Peek() == null)
			{
				Program.LogError("Condition without actions at end of file",
				                 ErrorOrigin.Parser);
				// never happens
				return null;
			}

			return conditions.Cast<OperationToken>().ToList();
		}

		private static List<Token> GetTokensList(ICollection<TokenType> separatorTypes,
		                                         ICollection<TokenType> endTokenTypes)
		{
			var tokens = new List<Token>();
			if (TokenIndex < Tokens.Count)
			{
				TokenType type = Tokens[TokenIndex].Type;
				if (endTokenTypes.Contains(type))
				{
					TokenIndex++;
				}
				else
				{
					EndTokenTypes.AddRange(separatorTypes);
					EndTokenTypes.AddRange(endTokenTypes);
					while (!EndTokenTypes.Contains(type) && TokenIndex < Tokens.Count)
					{
						Token token = NextExpression();
						if (token == null ||
						    separatorTypes.Contains(type) ||
						    EndTokenTypes.Contains(type))
						{
							return tokens;
						}

						tokens.Add(token);

						if (TokenIndex >= Tokens.Count)
						{
							break;
						}

						type = Tokens[TokenIndex].Type;
						TokenIndex++;
					}

					EndTokenTypes.RemoveRange(
						EndTokenTypes.Count - separatorTypes.Count - endTokenTypes.Count,
						separatorTypes.Count + endTokenTypes.Count);
				}
			}

			return tokens;
		}

		private static List<Token> GetTokensList(ICollection<TokenType> endTokenTypes)
		{
			var tokens = new List<Token>();
			if (TokenIndex < Tokens.Count)
			{
				TokenType type = Tokens[TokenIndex].Type;
				if (endTokenTypes.Contains(type))
				{
					TokenIndex++;
				}
				else
				{
					EndTokenTypes.AddRange(endTokenTypes);
					while (!EndTokenTypes.Contains(type) && TokenIndex < Tokens.Count)
					{
						Token token = NextExpression();
						if (token == null ||
						    EndTokenTypes.Contains(type))
						{
							return tokens;
						}

						tokens.Add(token);

						if (TokenIndex >= Tokens.Count)
						{
							break;
						}

						type = Tokens[TokenIndex].Type;
						TokenIndex++;
					}

					EndTokenTypes.RemoveRange(
						EndTokenTypes.Count - endTokenTypes.Count, endTokenTypes.Count);
				}
			}

			return tokens;
		}
	}
}