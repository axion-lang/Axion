using System;
using System.Collections.Generic;
using System.Linq;
using Axion.Tokens;

namespace Axion.Processing
{
	/// <summary>
	///     Builds tokens list into syntax tree.
	/// </summary>
	internal static class Parser
	{
		#region Variables

		// expressions splitted by newlines or semicolons
		private static List<string> endTokens = new List<string>
		{
			"#newline", ";", "#eof"
		};
		
		private static readonly List<Token> tokens = new List<Token>();
		private static int tokenIndex;

		#endregion

		/// <summary>
		///     Makes a tree from lexer's tokens list.
		/// </summary>
		internal static void Parse()
		{
			// copy tokens from script file
			tokens.AddRange(Program.OutFile.Tokens);
			// remove them from script file
			Program.OutFile.Tokens.Clear();
			// refill script file's tokens list with expressions
			while (tokenIndex < tokens.Count)
			{
				var expression = NextExpression();
				if (expression != null)
				{
					Program.OutFile.Tokens.Add(expression);
				}
				tokenIndex++;
			}
		}

		private static Token NextExpression(Token prevExpr = null)
		{
			if (tokenIndex >= tokens.Count)
			{
				return prevExpr;
			}

			var token = tokens[tokenIndex];
			if (endTokens.Contains(token.Value))
			{
				return prevExpr;
			}

			Token expression = null;

			switch (token.Type)
			{
				// literals are not expressions, merge them with some next token
				case TokenType.Identifier when prevExpr == null:
				case TokenType.Number when prevExpr == null:
				case TokenType.String when prevExpr == null:
				{
					expression = token;
					break;
				}

				case TokenType.Keyword:
				{
					switch (token.Value)
					{
						#region Import library (use)

						// use { lib1Expr, lib2Expr, etc }
						case "use" when tokens.Count > tokenIndex + 2 &&
										PeekNext()?.Value == "{":
						{
							// skip 'use {'
							tokenIndex += 2;
							List<Token> imports = GetExpressionsList(new[] { "}" }, new[] { "," });
							Program.OutFile.Imports.AddRange(imports);
							break;
						}

						#endregion

						#region Branch (if, elif, else, etc.)

						// if conditionExpr:
						//     action()
						case "if":
						{
							tokenIndex++;
							List<OperationToken> conditions = ParseConditions();
							List<Token> block = ParseBlock();
							expression = new BranchingToken(conditions, block);
							break;
						}
						// elif conditionExpr:
						//     action()
						case "elif" when prevExpr is BranchingToken parentBranch:
						{
							tokenIndex++;
							List<OperationToken> conditions = ParseConditions();
							List<Token> block = ParseBlock();
							parentBranch.ElseIfs.Add(conditions, block);
							expression = parentBranch;
							break;
						}
						// else:
						//     action()
						case "else" when prevExpr is BranchingToken parentBranch:
						{
							var nextToken = PeekNext();
							if (nextToken != null && nextToken.Value != ":")
							{
								Program.LogError("'else' block can't have any expressions before colon",
												 ErrorOrigin.Parser,
												 token.LinePosition,
												 token.ColumnPosition);
								return null;
							}

							// skip 'else:'
							tokenIndex += 2;
							List<Token> block = ParseBlock();
							parentBranch.ElseBlock.AddRange(block);
							return parentBranch;
						}

						#endregion

						#region Loop (while, foreach)
						/*
						// while conditionExpr, iteratorExpr:
						//     action()
						case "while":
						{
							var indexer = GetSingleExpressionUntil(",");
							if
							List<OperationToken> conditions = ParseConditions(new[] { ",", ":" });
							Token iteratorFunction = null;
							// then appending iterator function
							if (tokens[tokenIndex - 1].Value == ",")
							{
								iteratorFunction = NextExpression();
							}

							List<Token> block = ParseBlock();
							return new LoopToken(conditions, iteratorFunction, block);
						}

						// foreach identifierExpr in identifierExpr:
						//     action(var)
						case "foreach":
						{
							var conditions = ParseConditions();
							if (conditions.Count != 1 ||
								conditions[0].Operator != "in")
							{
								Program.LogError("Invalid 'foreach' conditions",
												 ErrorOrigin.Parser,
												 token.LinePosition,
												 token.ColumnPosition);
							}

							//var block = ParseBlock();
							break;
						}
						*/
						//unary keywords: next, out, etc.
						//case "next":
						//{
						//	return NextExpression();
						//}

						#endregion

						default:
						{
							Program.LogError($"Invalid position of keyword: '{token.Value}'",
											 ErrorOrigin.Parser,
											 token.LinePosition,
											 token.ColumnPosition);
							return null;
						}
					}

					break;
				}

				case TokenType.Operator:
				{
					var op = token.Value;
					switch (op)
					{
						#region Function call

						// functionName(arg1Expr, arg2Expr, etc)
						case "(" when tokenIndex >= 1 && tokens[tokenIndex - 1].Type == TokenType.Identifier:
						{
							var funcName = tokens[tokenIndex - 1];
							// skip (
							tokenIndex++;
							List<Token> arguments = GetExpressionsList(new[] { ")" }, new[] { "," });
							expression = new FunctionCallToken(funcName, arguments);
							break;
						}

						#endregion
						/*
						#region Collection indexer

						// collectionOrStringExpr[indexExpr]
						case "[" when prevExpr != null && tokens.Count > tokenIndex + 2:
						{
							if (prevExpr.Type == TokenType.String || prevExpr.Type == TokenType.Identifier)
							{
								var index = GetNextSingleExpressionUntil("]");
								expression = new IndexerToken(prevExpr, index);
							}
							else
							{
								Program.LogError($"Indexer cannot be applied to type: {prevExpr.Type:G}",
								                 ErrorOrigin.Parser,
								                 prevExpr.LinePosition,
								                 prevExpr.ColumnPosition);
								return null;
							}
							break;
						}

						#endregion
						*/
						#region Collection

						case "{" when prevExpr != null:
						{
							Token itemType;
							CollectionType collectionType;

							// array - type{ 1, 2, 3 }
							if (prevExpr.Type == TokenType.BuiltInType)
							{
								itemType = prevExpr;
								collectionType = CollectionType.Array;
							}
							// list - type*{ 1, 2, 3 }
							else if (prevExpr.Value == "*" && tokens.Count > 3)
							{
								itemType = tokens[tokenIndex - 3];
								collectionType = CollectionType.List;
							}
							// 'array(type) {}', 'list(type) {}', 'matrix(type) {}', etc.
							else if (!(prevExpr is FunctionCallToken collectionInitCallToken) ||
									 !Enum.TryParse(collectionInitCallToken.NameToken.Value, true, out collectionType))
							{
								Program.LogError("'{' is at invalid position",
												 ErrorOrigin.Parser,
												 token.LinePosition,
												 token.ColumnPosition);
								return null;
							}
							else
							{
								itemType = collectionInitCallToken;
							}

							List<Token> items = GetExpressionsList(new[] { "}" }, new[] { "," });
							expression = new CollectionToken(itemType, collectionType, items);
							break;
						}

						#endregion

						#region Operation

						// TODO add operations priority by '()'
						case "++":
						case "--":
						{
							if (tokenIndex != 0 && prevExpr != null)
							{
								// postfix (numExpr++, numExpr--)
								if (!Defs.LiteralTypes.Contains(prevExpr.Type))
								{
									Program.LogError(
										$"Invalid left operand: {prevExpr}",
										ErrorOrigin.Parser,
										prevExpr.LinePosition,
										prevExpr.ColumnPosition);
								}

								expression = new OperationToken(op, prevExpr, null);
							}
							else
							{
								// prefix (++numExpr, --numExpr)
								if (tokenIndex >= tokens.Count)
								{
									Program.LogError(
										$"Invalid position of '{op}' operator",
										ErrorOrigin.Parser,
										token.LinePosition,
										token.ColumnPosition);
								}

								tokenIndex++;
								var rightOperand = NextExpression();
								tokenIndex--;

								if (!Defs.LiteralTypes.Contains(rightOperand.Type))
								{
									Program.LogError(
										$"Invalid right operand: {rightOperand}",
										ErrorOrigin.Parser,
										rightOperand.LinePosition,
										rightOperand.ColumnPosition);
								}

								expression = new OperationToken(op, null, rightOperand);
							}
							break;
						}
						// "not expression"
						case "not":
						{
							var rightOperand = GetNextExpressionSkipBreaks("'not' operator without condition");
							return new OperationToken(op, null, rightOperand);
						}
						// binary operator
						default:
						{
							// check if left '=''s operand is identifier
							if (op == "=" && (tokenIndex < 1 || tokens[tokenIndex - 1].Type != TokenType.Identifier))
							{
								var invalidTarget = tokens[tokenIndex - 1];
								Program.LogError(
									$"Invalid assignment target: {invalidTarget}",
									ErrorOrigin.Parser,
									invalidTarget.LinePosition,
									invalidTarget.ColumnPosition);
							}

							var rightOperand = GetNextExpressionSkipBreaks("Binary operator without right operand");
							var operation = new OperationToken(op, prevExpr, rightOperand);
							// if last operation in chain
							if (tokenIndex >= tokens.Count || PeekNext()?.Type != TokenType.Operator)
							{
								return operation;
							}

							// else continue chain parsing
							expression = operation;
							break;
						}

						#endregion
					}

					break;
				}

				case TokenType.Unknown:
				{
					switch (token.Value)
					{
						case "#eof":
						{
							return prevExpr;
						}
						// these types skipped only if they aren't in EndTokenTypes
						case "#newline":
						case "#indent":
						case "#outdent":
						{
							expression = prevExpr;
							break;
						}
						default:
						{
							Program.LogWarning($"Invalid token: {token}",
											   ErrorOrigin.Parser,
											   token.LinePosition,
											   token.ColumnPosition);
							expression = token;
							break;
						}
					}

					break;
				}

				default:
				{
					Program.LogWarning($"Invalid position of token: {token}",
									   ErrorOrigin.Parser,
									   token.LinePosition,
									   token.ColumnPosition);
					expression = token;
					break;
				}
			}

			tokenIndex++;
			return NextExpression(expression);
		}

		private static Token PeekNext(int index = 1)
		{
			index += tokenIndex;
			return index < tokens.Count
				? tokens[index]
				: null;
		}

		/*
		private static Token GetNext(int index = 1)
		{
			tokenIndex += index;
			return tokenIndex < tokens.Count
				? tokens[tokenIndex]
				: null;
		}*/

		private static Token GetNextExpressionSkipBreaks(string failMsg)
		{
			do
			{
				tokenIndex++;
				if (tokenIndex == tokens.Count)
				{
					Program.LogError(failMsg,
					                 ErrorOrigin.Parser,
					                 tokens[tokenIndex - 1].LinePosition,
					                 tokens[tokenIndex - 1].ColumnPosition);
				}
			} while (endTokens.Contains(tokens[tokenIndex].Value));
			return NextExpression();
		}

		private static Token GetNextSingleExpressionUntil(params string[] endExprTokens)
		{
			tokenIndex++;
			endTokens.AddRange(endExprTokens);
			var expression = NextExpression();
			endTokens.RemoveRange(endTokens.Count - endExprTokens.Length, endExprTokens.Length);
			// skip end token
			tokenIndex++;
			return expression;
		}

		private static List<Token> GetExpressionsList(ICollection<string> listEndTypes, IEnumerable<string> separatorTypes = null)
		{
			var exprList = new List<Token>();
			if (tokenIndex < tokens.Count && !listEndTypes.Contains(tokens[tokenIndex].Value))
			{
				List<string> originalEndTokens = endTokens;
				endTokens = endTokens.Union(separatorTypes?.Union(listEndTypes) ?? listEndTypes).ToList();

				for (; tokenIndex < tokens.Count; tokenIndex++)
				{
					var token = NextExpression();
					if (token != null)
					{
						exprList.Add(token);
					}
					bool atEnd = listEndTypes.Contains(tokens[tokenIndex].Value);

					if (!atEnd)
					{
						if (tokenIndex >= tokens.Count)
						{
							Program.LogError($"Unexpected end of file. Expected ({string.Join(", ", listEndTypes)})", ErrorOrigin.Parser);
						}
					}
					else
					{
						break;
					}
				}

				endTokens = originalEndTokens;
			}
			return exprList;
		}

		private static List<Token> ParseBlock()
		{
			List<Token> blockTokens;
			// process multiline block (until outdent)
			if (PeekNext()?.Value == "#newline" &&
				tokenIndex + 2 < tokens.Count &&
				PeekNext(2).Value == "#indent")
			{
				// skip newline & indent
				tokenIndex += 2;
				blockTokens = GetExpressionsList(new[] { "#outdent" });
			}
			// single line block (until first semicolon)
			else
			{
				blockTokens = GetExpressionsList(new string[0]);
			}

			while (tokens.Count > tokenIndex && endTokens.Contains(tokens[tokenIndex].Value))
			{
				tokenIndex++;
			}

			return blockTokens;
		}

		private static List<OperationToken> ParseConditions(IEnumerable<string> stopTokens = null)
		{
			List<Token> conditions = GetExpressionsList((stopTokens?.Union(new[] { ":" })
													?? new[] { ":" }).ToArray());

			// if some condition doesn't return a bool
			var invalidCondition = conditions.FirstOrDefault(condition =>
																 !(condition is OperationToken) ||
																 condition.Type != TokenType.Identifier);
			if (invalidCondition != null)
			{
				Program.LogError("Invalid condition",
								 ErrorOrigin.Parser,
								 invalidCondition.LinePosition,
								 invalidCondition.ColumnPosition);
			}

			// condition at end of file
			if (PeekNext() == null)
			{
				Program.LogError("Condition without actions at end of file",
								 ErrorOrigin.Parser);
				// never happens
				return null;
			}

			return conditions.Cast<OperationToken>().ToList();
		}
	}
}