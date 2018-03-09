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

		private static readonly Regex RegexIdStart = new Regex("[_a-zA-Z]");
		private static readonly Regex RegexId = new Regex("[_a-zA-Z0-9]");
		private static readonly Regex RegexNumStart = new Regex("[0-9]");
		private static readonly Regex RegexNum = new Regex("[0-9`BSLbsl]"); // B - byte, S - short, L - long 

		private static readonly char[] OperatorChars =
		{
			'+', '-', '*', '/', '%', '!', '>', '<', '=', '|', '&', '^', '~', '.'
		};

		private static readonly string[] AllowedOperators =
		{
			// default
			"=", "+", "-", "*", "/", "%", "**", ".",
			// self-assignment
			"+=", "-=", "*=", "/=", "%=", "**=", "++", "--",
			// comparison
			"!", ">", "<", ">=", "<=", "==", "!=",
			// bitwise
			"|", "^", "&", "~",
			"|=", "^=", "&=", "~="
		};

		private static readonly Dictionary<char, Token> SpecialTokens = new Dictionary<char, Token>
		{
			{ '(', new Token(TokenType.OpenParenthese) },
			{ ')', new Token(TokenType.CloseParenthese) },
			{ '[', new Token(TokenType.OpenBracket) },
			{ ']', new Token(TokenType.CloseBracket) },
			{ '{', new Token(TokenType.OpenCurly) },
			{ '}', new Token(TokenType.CloseCurly) },
			{ ',', new Token(TokenType.Comma) },
			{ ':', new Token(TokenType.Colon) },
			{ ';', new Token(TokenType.Semicolon) }
		};

		private static readonly string[] Keywords =
		{
            // loops
            "for", "while", "out", "next",
            // conditions
            "if", "elif", "else", "switch", "case",
			"is", "not", "and", "or",
            // variables
            "new", "delete", "null", "as",
            // errors
            "try", "catch", "throw",
            // Objects && access modifiers
            "module", "global", "local", "inner",
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

			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];

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
					Tokens.Add(new Token(TokenType.Indent));
					lineIndentLevel++;
				}
				// if indent decreased
				while (tabsCount < lineIndentLevel)
				{
					Tokens.Add(new Token(TokenType.Outdent));
					lineIndentLevel--;
				}

				#endregion

				for (var charIndex = 0; charIndex < line.Length; charIndex++)
				{
					var ch = line[charIndex];

					// whitespaces
					if (ch == ' ' || ch == '\t')
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
						if (!AllowedOperators.Contains(op))
						{
							Program.LogError($"Unknown operator: {op} ", ErrorOrigin.Lexer, true, lineIndex, charIndex);
						}
						Tokens.Add(new Token(TokenType.Operator, operatorBuilder.ToString()));
					}
					// special operators
					else if (SpecialTokens.ContainsKey(ch))
					{
						SpecialTokens.TryGetValue(ch, out Token SpecialToken);
						Tokens.Add(SpecialToken);
					}
					// string
					else if (ch == '\"' || ch == '\'')
					{
						if (line.Length > charIndex + 2 && ch.ToString() + line[charIndex + 1] + line[charIndex + 2] == "\"\"\"")
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
						var identifier = identifierBuilder.ToString();

						// special keyword
						if (identifier == "use")
						{
							var references = line.Substring(charIndex).Split(',');
							for (var i = 0; i < references.Length; i++)
							{
								Tokens.Add(new Token(TokenType.Reference, references[i].Trim()));
							}

							break;
						}
						if (BuiltInTypes.Contains(identifier))
						{
							Tokens.Add(new Token(TokenType.BuiltInType, identifier));
						}
						else
						{
							Tokens.Add(Keywords.Contains(identifier)
								? new Token(TokenType.Keyword, identifier)
								: new Token(TokenType.Identifier, identifier));
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
		}

		private static Token ParseString(string delimiter, IReadOnlyList<string> lines, ref int charIndex, ref int lineIndex)
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
						if (line.Length - charIndex < delimiter.Length || line.Substring(charIndex, delimiter.Length) != delimiter)
						{
							stringBuilder.Append(line[charIndex]);
						}
						else if (charIndex == 0 || line[charIndex - 1] != '\\')
						{
							charIndex += delimiter.Length;
							return new Token(TokenType.String, stringBuilder.ToString());
						}
					}
					charIndex = 0;
				}
			}
			else // " or '
			{
				for (; charIndex < line.Length; charIndex++)
				{
					if (line.Length - charIndex < delimiter.Length || line.Substring(charIndex, delimiter.Length) != delimiter)
					{
						stringBuilder.Append(line[charIndex]);
					}
					else if (charIndex == 0 || line[charIndex - 1] != '\\')
					{
						charIndex += delimiter.Length;
						return new Token(TokenType.String, stringBuilder.ToString());
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
			var number = numberBuilder.ToString().ToLower();

			// float number
			if (number.Contains("."))
			{
				// long float
				if (number.EndsWith("`l"))
				{
					if (!double.TryParse(number.Replace("`l", ""), out var longFloat))
					{
						Program.LogError("Invalid 'lfloat' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_LFloat, longFloat.ToString());
					}
				}
				// float
				else
				{
					if (!float.TryParse(number, out var @float))
					{
						Program.LogError("Invalid 'float' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Float, @float.ToString());
					}
				}
			}
			// integer
			else
			{
				// byte
				if (number.EndsWith("`b"))
				{
					if (!byte.TryParse(number.Replace("`b", ""), out var @byte))
					{
						Program.LogError("Invalid 'byte' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Byte, @byte.ToString());
					}
				}
				// short
				else if (number.EndsWith("`s"))
				{
					if (!short.TryParse(number.Replace("`s", ""), out var sint))
					{
						Program.LogError("Invalid 'sint' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_SInt, sint.ToString());
					}
				}
				// long
				else if (number.EndsWith("`l"))
				{
					if (!long.TryParse(number.Replace("`l", ""), out var lint))
					{
						Program.LogError("Invalid 'lint' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_LInt, lint.ToString());
					}
				}
				// integer
				else
				{
					if (!int.TryParse(number, out var @int))
					{
						Program.LogError("Invalid 'int' value", ErrorOrigin.Lexer, true, lineIndex, charIndex);
					}
					else
					{
						return new Token(TokenType.Number_Int, @int.ToString());
					}
				}
			}
			return null;
		}
	}
}
