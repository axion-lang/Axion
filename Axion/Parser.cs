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
		private static readonly List<TokenType> EndTokenTypes = new List<TokenType>
		{
			TokenType.Newline,
			TokenType.Semicolon,
			TokenType.EOF
		};

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
			string    value = token.Value;

			if (EndTokenTypes.Contains(type))
			{
				return previousToken;
			}

			TokenIndex++;

			switch (type)
			{
				case TokenType.Identifier when previousToken == null:
				case TokenType.Number when previousToken == null:
				case TokenType.String when previousToken == null:
				{
					return NextExpression(token);
				}
				case TokenType.Keyword:
				{
					switch (value)
					{
						case "if":
						{
							var conditions = ParseIfConditions();
							var block      = ParseBlock();
							return NextExpression(new BranchingToken(conditions, block));
						}
						case "elif" when previousToken is BranchingToken parentBranch:
						{
							var conditions = ParseIfConditions();
							var block      = ParseBlock();
							parentBranch.ElseIfs.Add(conditions, block);
							return NextExpression(parentBranch);
						}
						case "else" when previousToken is BranchingToken parentBranch:
						{
							if (Peek().Type != TokenType.Colon)
							{
								Program.LogError("'else' block can't have any tokens before colon",
								                 ErrorOrigin.Parser, true,
								                 token.LinePosition,
								                 token.ColumnPosition);
								return null;
							}

							// skiping 'else' colon because there is no conditions
							TokenIndex++;

							var block = ParseBlock();
							// offset
							TokenIndex--;
							parentBranch.ElseTokens.AddRange(block);
							return parentBranch;
						}
						default:
						{
							Program.LogError($"Invalid position of keyword: '{value}'",
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

					var items = GetTokensList(new[] { TokenType.Comma }, new[] { TokenType.CloseCurly });

					return NextExpression(new CollectionToken(itemType, collectionType, items));
				}
				// Operation TODO add operands on different lines support + unary operators + operations priority
				case TokenType.Operator when previousToken?.Type != TokenType.BuiltInType:
				{
					Token rightOperand = NextExpression();
					return NextExpression(new OperationToken(value, previousToken, rightOperand));
				}
				// Function call
				case TokenType.OpenParenthese when previousToken != null && previousToken.Type == TokenType.Identifier:
				{
					var arguments = GetTokensList(new[] { TokenType.Comma }, new[] { TokenType.CloseParenthese });
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
			}
			// single line block (until first semicolon)
			else
			{
				tokens = GetTokensList(new[] { TokenType.Newline, TokenType.Semicolon });
			}

			while (Tokens.Count > TokenIndex && EndTokenTypes.Contains(Tokens[TokenIndex].Type))
			{
				TokenIndex++;
			}

			return tokens;
		}

		private static List<OperationToken> ParseIfConditions()
		{
			var conditions = GetTokensList(new[] { TokenType.Colon });

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
					while (!endTokenTypes.Contains(type) && TokenIndex < Tokens.Count)
					{
						Token token = NextExpression();
						if (token == null ||
						    endTokenTypes.Contains(type))
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
						endTokenTypes.Count);
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
					while (!endTokenTypes.Contains(type) && TokenIndex < Tokens.Count)
					{
						Token token = NextExpression();
						if (token == null ||
						    endTokenTypes.Contains(type))
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