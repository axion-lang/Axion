using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Axion.Tokens;

namespace Axion.Processing
{
	internal static class Lexer
	{
		internal static readonly List<Token> Tokens = Program.OutFile.Tokens;
		private static char[] chars;
		private static int debugLIndex = 1;
		private static int debugCIndex;
		private static int cIndex;
		private static int lastIndentLevel;

		public static void Tokenize(char[] input)
		{
			chars = input;

			for (cIndex = 0; cIndex < chars.Length; Move(1))
			{
				var c = chars[cIndex];

				#region Trash: ' ', '\t', '\r'

				if (c == ' ' || c == '\t' || c == '\r')
				{
					continue;
				}

				#endregion

				#region Newline

				if (c == '\n')
				{
					debugLIndex++;
					debugCIndex = 0;
					// skip multiple newlines
					if (Tokens.Count > 1 && Tokens.Last().Value != "#newline")
					{
						Tokens.Add(new Token("#newline"));
					}

					#region Block body creation (indent/outdent)

					// TODO add indentation with spaces
					var nextC = PeekNext();
					if (nextC != '\r' && nextC != '\n')
					{
						var tabsCount = 0;
						while (PeekNext() == '\t')
						{
							Move(1);
							tabsCount++;
						}

						if (tabsCount > lastIndentLevel)
						{
							for (; tabsCount > lastIndentLevel; lastIndentLevel++)
							{
								Tokens.Add(new Token("#indent", lastIndentLevel, debugLIndex));
							}
						}
						else
						{
							for (; tabsCount < lastIndentLevel; lastIndentLevel--)
							{
								Tokens.Add(new Token("#outdent", lastIndentLevel, debugLIndex));
							}
						}
					}

					#endregion
				}

				#endregion

				#region Comment

				else if (c == '`')
				{
					// multiline
					if (cIndex + 1 < chars.Length && chars[cIndex + 1] == '`')
					{
						while (cIndex + 1 < chars.Length
							   // skip all until next ``:
							   && chars[cIndex] + chars[cIndex + 1].ToString() != "``")
						{
							Move(1);
						}
					}
					// single-line / inline
					else
					{
						// skip `
						Move(1);
						while (cIndex < chars.Length
							   // skip all until next `
							   && chars[cIndex] != '`'
							   // break at newline chars
							   && chars[cIndex] != '\r'
							   && chars[cIndex] != '\n')
						{
							Move(1);
						}
					}
				}

				#endregion

				#region Operator

				else if (Defs.OperatorChars.Contains(c))
				{
					var operatorStartIndex = debugCIndex;
					var op = Defs.SpecialOperators.Contains(c.ToString()) 
						? c.ToString() 
						: GetLiteral(Defs.OperatorChars.Contains);

					if (!Defs.ValidOperators.Contains(op))
					{
						Program.LogError($"Invalid operator: '{op}'", ErrorOrigin.Lexer, debugLIndex, operatorStartIndex);
					}

					Tokens.Add(new Token(op, debugLIndex, operatorStartIndex, TokenType.Operator));
				}

				#endregion

				#region String

				else if (c == '"' || c == '\'')
				{
					var delimiter = c.ToString();
					if (chars.Length > cIndex + 2)
					{
						var next3chars =
							$"{chars[cIndex]}{chars[cIndex + 1]}{chars[cIndex + 2]}";
						// check next 3 chars for multiline delimiter
						if (next3chars == "\"\"\"" || next3chars == "'''")
						{
							delimiter = next3chars;
						}
					}

					Tokens.Add(ParseString(delimiter));
				}

				#endregion

				#region Number

				else if (char.IsDigit(c))
				{
					Tokens.Add(ParseNumber());
				}

				#endregion

				#region Keyword / Built-in type / Identifier

				else if (char.IsLetter(c) || c == '_')
				{
					Tokens.Add(ParseIdentifier());
				}

				#endregion

				else
				{
					Program.LogError($"Unknown character: '{c}'", ErrorOrigin.Lexer, debugLIndex, debugCIndex);
				}
			}

			Tokens.Add(new Token("#eof"));
		}

		private static Token ParseString(string delimiter)
		{
			var stringStartCIndex = debugCIndex;
			var stringStartLIndex = debugLIndex;
			var str = "";
			// skipping string open delimiter
			Move(delimiter.Length);

			for (; cIndex < chars.Length; Move(1))
			{
				var c = chars[cIndex];

				// get next piece of string to compare it with delimiter
				var nextCharsString = "";
				for (var i = 0; i < delimiter.Length; i++)
				{
					if (cIndex + i >= chars.Length)
					{
						Program.LogError("Unclosed string", ErrorOrigin.Lexer, debugLIndex, debugCIndex);
					}

					nextCharsString += chars[cIndex + i];
				}

				if (nextCharsString == delimiter &&
					chars[cIndex - 1] != '\\')
				{
					// skipping string close delimiter ( - 1 as offset )
					Move(delimiter.Length - 1);
					return new Token(str, stringStartLIndex, stringStartCIndex, TokenType.String);
				}

				if (c == '\n')
				{
					debugLIndex++;
					debugCIndex = 0;
				}

				str += c;
			}

			// if not passed, there is some error
			Program.LogError("Unclosed string", ErrorOrigin.Lexer, debugLIndex, debugCIndex);
			return null;
		}

		private static Token ParseNumber()
		{
			var numberStartIndex = debugCIndex;
			var number = GetLiteral(char.IsLetterOrDigit);

			number = number.ToLower();
			var numberWithoutPostfix = char.IsDigit(number.Last())
				? number
				: number.Remove(number.Length - 1);

			// TODO add check if number not in it's type's values range
			#region Float number

			if (number.Contains("."))
			{
				// float
				if (float.TryParse(number, out var @float))
				{
					return new NumberToken(NumberType.Float, @float.ToString(CultureInfo.InvariantCulture), debugLIndex,
										   numberStartIndex);
				}

				// long float
				if (double.TryParse(numberWithoutPostfix, out var longFloat))
				{
					return new NumberToken(NumberType.LFloat, longFloat.ToString(CultureInfo.InvariantCulture), debugLIndex,
										   numberStartIndex);
				}
			}

			#endregion

			#region Integer

			else
			{
				//byte
				if (number.EndsWith("b") && byte.TryParse(numberWithoutPostfix, out var @byte))
				{
					return new NumberToken(NumberType.Byte, @byte.ToString(), debugLIndex, numberStartIndex);
				}

				// short
				if (number.EndsWith("s") && short.TryParse(numberWithoutPostfix, out var shortInt))
				{
					return new NumberToken(NumberType.SInt, shortInt.ToString(), debugLIndex, numberStartIndex);
				}

				// integer
				if (number == numberWithoutPostfix && int.TryParse(numberWithoutPostfix, out var integer))
				{
					return new NumberToken(NumberType.Int, integer.ToString(), debugLIndex, numberStartIndex);
				}

				// long
				if (long.TryParse(numberWithoutPostfix, out var longInt))
				{
					return new NumberToken(NumberType.LInt, longInt.ToString(), debugLIndex, numberStartIndex);
				}
			}

			#endregion

			Program.LogError("Invalid number literal", ErrorOrigin.Lexer, debugLIndex, numberStartIndex);
			return null;
		}

		private static Token ParseIdentifier()
		{
			var wordStartIndex = debugCIndex;
			var word = GetLiteral(char.IsLetterOrDigit);

			TokenType type;
			// check for literal operators 'and', 'or', 'in'...
			if (Defs.ConditionalOperators.Contains(word))
			{
				type = TokenType.Operator;
			}
			else if (Defs.BuiltInTypes.Contains(word))
			{
				type = TokenType.BuiltInType;
			}
			else if (Defs.Keywords.Contains(word))
			{
				type = TokenType.Keyword;
			}
			else
			{
				type = TokenType.Identifier;
			}

			return new Token(word, debugLIndex, wordStartIndex, type);
		}

		private static string GetLiteral(Func<char, bool> condition)
		{
			var literal = "";
			while (cIndex < chars.Length && condition(chars[cIndex]))
			{
				literal += chars[cIndex];
				Move(1);
			}
			// offset
			Move(-1);
			return literal;
		}

		private static void Move(int index)
		{
			cIndex += index;
			debugCIndex += index;
		}

		private static char PeekNext(int index = 0)
		{
			return cIndex + 1 + index < chars.Length
				? chars[cIndex + 1 + index]
				: '\0';
		}
	}
}