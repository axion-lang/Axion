using System;
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

		private static readonly Regex RegexIdentifierStart = new Regex("[_a-zA-Z]");
		private static readonly Regex RegexIdentifier = new Regex("[_a-zA-Z0-9]");

		private static readonly Regex RegexNumberStart = new Regex("[0-9]");
		private static readonly Regex RegexNumber = new Regex("[0-9`BSLbsl]"); // B - byte, S - short, L - long 

		private static readonly char[] OperatorChars =
		{
			':', '+', '-', '*', '/', '%',
			'!', '>', '<', '=', '|', '&',
			'^', '~', '.'
		};

		private static readonly string[] AllowedOperators =
		{
			// default
			":", "=", "+", "-", "*", "/", "%", "**", ".",
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
			{ ',', new Token(TokenType.Comma) },
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
            "module", "open", "local", "inner",
			"class", "struct", "enum", "self"
		};

		private static readonly string[] BuiltInTypes =
		{
			"bool", "byte", "sint", "int", "lint",
			"float", "lfloat", "str"
		};

		private static int LineIndex;

		public static void Tokenize(string[] lines)
		{
			var lineIndentLevel = 0;

			for (; LineIndex < lines.Length; LineIndex++)
			{
				var line = lines[LineIndex];

				// if line empty or commented - skipping it
				if (string.IsNullOrWhiteSpace(line) ||
					line.Trim().StartsWith("#"))
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

					if (ch == ' ' || ch == '\t')
					{
						continue;
					}
					// TODO add inline comments
					if (ch == '#')
					{
						while (charIndex < line.Length && line[charIndex] != '#')
						{
							charIndex++;
						}
					}

					// operator
					if (OperatorChars.Contains(ch))
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
							Program.LogError($"Unknown operator: {op} ", true, LineIndex, charIndex);
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
					else if (ch == '"' || ch == '\'')
					{
						Tokens.Add(ParseString(ch, ref line, ref charIndex));
					}
					// number
					else if (RegexNumberStart.IsMatch(ch.ToString()))
					{
						Tokens.Add(ParseNumber(ref line, ref charIndex));
						charIndex--;
					}
					// keyword / built-in type / identifier
					else if (RegexIdentifierStart.IsMatch(ch.ToString()))
					{
						var identifierBuilder = new StringBuilder();
						while (charIndex < line.Length && RegexIdentifier.IsMatch(line[charIndex].ToString()))
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
						Program.LogError($"Unknown character: {ch} ", false, LineIndex, charIndex);
					}
				}
				if (LineIndex != lines.Length - 1)
				{
					Tokens.Add(new Token(TokenType.Newline));
				}
			}
			LineIndex = 0;
		}

		private static Token ParseString(char delimiter, ref string line, ref int charIndex)
		{
			// TODO add multiline string support
			var stringBuilder = new StringBuilder();
			// skipping string delimiter
			charIndex++;
			try
			{
				// while char isn't unescaped delimiter
				while (!(line[charIndex] == delimiter && line[charIndex - 1] != '\\'))
				{
					stringBuilder.Append(line[charIndex]);
					charIndex++;
				}
			}
			catch (IndexOutOfRangeException)
			{
				Program.LogError("Unclosed string", true, LineIndex, charIndex);
			}
			return new Token(TokenType.String, stringBuilder.ToString());
		}

		private static Token ParseNumber(ref string line, ref int charIndex)
		{
			var numberBuilder = new StringBuilder();
			while (charIndex < line.Length && RegexNumber.IsMatch(line[charIndex].ToString()))
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
						Program.LogError("Invalid 'lfloat' value", true, LineIndex, charIndex);
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
						Program.LogError("Invalid 'float' value", true, LineIndex, charIndex);
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
						Program.LogError("Invalid 'byte' value", true, LineIndex, charIndex);
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
						Program.LogError("Invalid 'sint' value", true, LineIndex, charIndex);
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
						Program.LogError("Invalid 'lint' value", true, LineIndex, charIndex);
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
						Program.LogError("Invalid 'int' value", true, LineIndex, charIndex);
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
