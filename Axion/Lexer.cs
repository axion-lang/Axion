using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Axion.Tokens;

namespace Axion
{
	internal static class Lexer
	{
		public static readonly List<Token> Tokens = new List<Token>();

		#region Regular expressions

		private static readonly Regex RegexIdStart  = new Regex("[_a-zA-Z]");
		private static readonly Regex RegexId       = new Regex("[_a-zA-Z0-9]");
		private static readonly Regex RegexNumStart = new Regex("[0-9]");
		private static readonly Regex RegexNum      = new Regex("[0-9BSLbsl]"); // B - byte, S - short, L - long 

		#endregion

		#region Operators

		private static readonly char[] OperatorChars =
		{
			'+', '-', '*', '/', '%', '!', '>', '<', '=', '|', '&', '^', '~', '.', '?'
		};

		private static readonly string[] AssigningOperators =
		{
			// default
			"=", "+", "-", "*", "/", "%", "**", ".",
			// self-assignment
			"+=", "-=", "*=", "/=", "%=", "**=", "++", "--",
			// bitwise
			"|", "^", "&", "~",
			"|=", "^=", "&=", "~="
		};

		internal static readonly string[] ConditionalOperators =
		{
			"!", ">", "<", ">=", "<=", "==", "!=", "&&", "||", "?"
		};

		#endregion

		private static readonly Dictionary<char, TokenType> SpecialTypes = new Dictionary<char, TokenType>
		{
			{ '(', TokenType.OpenParenthese },
			{ ')', TokenType.CloseParenthese },
			{ '[', TokenType.OpenBracket },
			{ ']', TokenType.CloseBracket },
			{ '{', TokenType.OpenCurly },
			{ '}', TokenType.CloseCurly },
			{ ',', TokenType.Comma },
			{ ':', TokenType.Colon },
			{ ';', TokenType.Semicolon }
		};

		private static readonly string[] Keywords =
		{
			// loops
			"for", "while", "out", "next",
			// conditions
			"if", "elif", "else", "switch", "case", "other",
			// variables
			"new", "delete", "null", "as",
			// errors
			"try", "catch", "throw",
			// Objects & access modifiers
			"module", "global", "inner", "local",
			"class", "struct", "enum", "self"
		};

		private static readonly string[] BuiltInTypes =
		{
			"bool", "byte", "sint", "int", "lint",
			"float", "lfloat", "str"
		};

		public static void Tokenize(string[] lines)
		{
			var lineIndentLevel = 0;

			for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				string line = lines[lineIndex];

				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				#region indentation (creates block bodies)

				var tabsCount = 0;
				while (line[tabsCount] == '\t')
				{
					tabsCount++;
				}

				// if indent increased
				while (tabsCount > lineIndentLevel)
				{
					Tokens.Add(new Token(TokenType.Indent, null, lineIndex));
					lineIndentLevel++;
				}

				// if indent decreased
				while (tabsCount < lineIndentLevel)
				{
					Tokens.Add(new Token(TokenType.Outdent, null, lineIndex));
					lineIndentLevel--;
				}

				#endregion

				for (var charIndex = 0; charIndex < line.Length; charIndex++)
				{
					char ch = line[charIndex];

					// whitespaces
					if (ch == ' ' ||
					    ch == '\t')
					{
						// to next char
					}
					// inline / endline comments
					else if (ch == '#')
					{
						charIndex++;
						while (charIndex < line.Length && line[charIndex] != '#')
						{
							charIndex++;
						}
					}
					// operator
					else if (OperatorChars.Contains(ch))
					{
						var operatorBuilder = new StringBuilder();
						while (charIndex < line.Length && OperatorChars.Contains(line[charIndex]))
						{
							operatorBuilder.Append(line[charIndex]);
							charIndex++;
						}

						// return to previous char ( in cases 'functionCall(1+2)' )
						charIndex--;
						string op = operatorBuilder.ToString();
						if (!AssigningOperators.Contains(op) &&
						    !ConditionalOperators.Contains(op))
						{
							Program.LogError($"Unknown operator: {op} ", ErrorOrigin.Lexer, true, lineIndex, charIndex);
						}

						Tokens.Add(new Token(TokenType.Operator, operatorBuilder.ToString(), lineIndex, charIndex));
					}
					// special operators
					else if (SpecialTypes.ContainsKey(ch))
					{
						SpecialTypes.TryGetValue(ch, out TokenType SpecialType);
						Tokens.Add(new Token(SpecialType, null, lineIndex, charIndex));
					}
					// string
					else if (ch == '\"' ||
					         ch == '\'')
					{
						if (line.Length > charIndex + 2 &&
						    ch.ToString() + line[charIndex + 1] + line[charIndex + 2] == "\"\"\"")
						{
							Tokens.Add(ParseString("\"\"\"", lines, ref charIndex, ref lineIndex));
						}
						else
						{
							Tokens.Add(ParseString(ch.ToString(), lines, ref charIndex, ref lineIndex));
						}
					}
					// number
					else if (RegexNumStart.IsMatch(ch.ToString()))
					{
						Tokens.Add(ParseNumber(ref line, ref charIndex, ref lineIndex));
						charIndex--;
					}
					// keyword / built-in type / identifier
					else if (RegexIdStart.IsMatch(ch.ToString()))
					{
						var identifierBuilder = new StringBuilder();
						while (charIndex < line.Length && RegexId.IsMatch(line[charIndex].ToString()))
						{
							identifierBuilder.Append(line[charIndex]);
							charIndex++;
						}

						string identifier = identifierBuilder.ToString();

						// special keyword
						if (identifier == "use")
						{
							string[] references = line.Substring(charIndex).Split(',');
							for (var i = 0; i < references.Length; i++)
							{
								Tokens.Add(new Token(TokenType.Reference, references[i].Trim(), lineIndex, charIndex));
							}

							break;
						}

						if (BuiltInTypes.Contains(identifier))
						{
							Tokens.Add(new Token(TokenType.BuiltInType, identifier, lineIndex, charIndex));
						}
						else
						{
							/*
							Tokens.Add(Enum.TryParse(identifier, true, out TokenType KeywordType)
										   ? new Token(KeywordType, null, lineIndex, charIndex)
										   : new Token(TokenType.Identifier, identifier, lineIndex, charIndex));*/
							Tokens.Add(Keywords.Contains(identifier)
								           ? new Token(TokenType.Keyword, identifier, lineIndex, charIndex)
								           : new Token(TokenType.Identifier, identifier, lineIndex, charIndex));
						}

						charIndex--;
					}
					else
					{
						Program.LogError($"Unknown character: {ch} ", ErrorOrigin.Lexer, false, lineIndex, charIndex);
					}
				}

				if (lineIndex != lines.Length - 1)
				{
					Tokens.Add(new Token(TokenType.Newline));
				}
			}

			Tokens.Add(new Token(TokenType.EOF));
		}

		private static Token ParseString(string delimiter, IReadOnlyList<string> lines, ref int charIndex,
		                                 ref int lineIndex)
		{
			var stringBuilder = new StringBuilder();
			// skipping string delimiter
			charIndex += delimiter.Length;
			string line = lines[lineIndex];
			if (delimiter.Length == 3) // """ , not anything else
			{
				for (; lineIndex < lines.Count; lineIndex++)
				{
					line = lines[lineIndex];
					for (; charIndex < line.Length; charIndex++)
					{
						if (line.Length - charIndex < delimiter.Length ||
						    line.Substring(charIndex, delimiter.Length) != delimiter)
						{
							stringBuilder.Append(line[charIndex]);
						}
						else if (charIndex == 0 ||
						         line[charIndex - 1] != '\\')
						{
							charIndex += delimiter.Length - 1;
							return new Token(TokenType.String, stringBuilder.ToString(), lineIndex, charIndex);
						}
					}

					charIndex = 0;
				}
			}
			else // " or '
			{
				for (; charIndex < line.Length; charIndex++)
				{
					if (line.Length - charIndex < delimiter.Length ||
					    line.Substring(charIndex, delimiter.Length) != delimiter)
					{
						stringBuilder.Append(line[charIndex]);
					}
					else if (charIndex == 0 ||
					         line[charIndex - 1] != '\\')
					{
						charIndex += delimiter.Length - 1;
						return new Token(TokenType.String, stringBuilder.ToString(), lineIndex, charIndex);
					}
				}
			}

			// if not passed, there is some error
			Program.LogError("Unclosed string", ErrorOrigin.Lexer, true, lineIndex, charIndex);
			return null;
		}

		private static Token ParseNumber(ref string line, ref int charIndex, ref int lineIndex)
		{
			var numberBuilder = new StringBuilder();
			while (charIndex < line.Length && RegexNum.IsMatch(line[charIndex].ToString()))
			{
				numberBuilder.Append(line[charIndex]);
				charIndex++;
			}

			string number = numberBuilder.ToString().ToLower();

			// float number
			if (number.Contains("."))
			{
				// long float
				if (number.EndsWith("l"))
				{
					if (!double.TryParse(number.Replace("l", ""), out double longFloat))
					{
						Program.LogError("Invalid 'lfloat' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_LFloat, longFloat.ToString(), lineIndex, charIndex);
					}
				}
				// float
				else
				{
					if (!float.TryParse(number, out float @float))
					{
						Program.LogError("Invalid 'float' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Float, @float.ToString(), lineIndex, charIndex);
					}
				}
			}
			// integer
			else
			{
				// byte
				if (number.EndsWith("b"))
				{
					if (!byte.TryParse(number.Replace("b", ""), out byte @byte))
					{
						Program.LogError("Invalid 'byte' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Byte, @byte.ToString(), lineIndex, charIndex);
					}
				}
				// short
				else if (number.EndsWith("s"))
				{
					if (!short.TryParse(number.Replace("s", ""), out short sint))
					{
						Program.LogError("Invalid 'sint' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_SInt, sint.ToString(), lineIndex, charIndex);
					}
				}
				// long
				else if (number.EndsWith("l"))
				{
					if (!long.TryParse(number.Replace("l", ""), out long lint))
					{
						Program.LogError("Invalid 'lint' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_LInt, lint.ToString(), lineIndex, charIndex);
					}
				}
				// integer
				else
				{
					if (!int.TryParse(number, out int @int))
					{
						Program.LogError("Invalid 'int' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Int, @int.ToString(), lineIndex, charIndex);
					}
				}
			}

			return null;
		}
	}
}