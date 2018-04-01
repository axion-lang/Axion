using System.Linq;
using Axion.Tokens;

namespace Axion
{
	/// <summary>
	/// Static class, contains all language's syntax definitions (allowed operators, keywords, etc.)
	/// </summary>
	internal static class Defs
	{
		internal static readonly char[] OperatorChars =
		{
			'+', '-', '*', '/', '%',
			'!', '>', '<', '=', '|',
			'&', '^', '~', '.',
			'{', '}', '[', ']',
			'(', ')', ',', ':', ';'
		};

		internal static readonly string[] AssigningOperators =
		{
			// standard
			"=", "+", "-", "*", "**", "/", "//", "%",
			".",
			// standard self-assignment
			"+=", "-=", "*=", "**=", "/=", "//=", "%=",
			"++", "--",

			// bitwise
			"|", "^", "&", "~",
			// bitwise self-assignment
			"|=", "^=", "&=", "~="
		};

		internal static readonly string[] ConditionalOperators =
		{
			">", "<", ">=", "<=", "==", "!=",
			"not", "and", "or", "in"
		};

		internal static readonly string[] SpecialOperators =
		{
			"{", "}", "[", "]", "(", ")", ",", ":", ";"
		};

		internal static readonly string[] ValidOperators = AssigningOperators
				.Union(ConditionalOperators)
				.Union(SpecialOperators)
				.ToArray();

		internal static readonly TokenType[] LiteralTypes =
		{
			TokenType.Identifier,
			TokenType.Number,
			TokenType.String
		};

		internal static readonly string[] Keywords =
		{
			// loops
			"while", "foreach", "out", "next",
			// conditions
			"if", "elif", "else",
			"switch", "case", "other",
			// variables
			"new", "delete", "as", "ref",
			"true", "false", "null",
			// errors
			"try", "catch", "anyway", "raise",
			// imports
			"use", "module",
			// modifiers
			"global", "inner", "local", "readonly", "static",
			"async", "await",
			// definitions
			"class", "struct", "enum", "self",
			//
			"return", "yield"
		};

		internal static readonly string[] BuiltInTypes =
		{
			"bool", "byte", "sint", "int", "lint",
			"float", "lfloat", "str"
		};

		#region Regular Expressions

		/* TODO add number regex checks
	    private static readonly Regex Hexnumber = new Regex("0[xX](?:_?[0-9a-fA-F])+");
	    private static readonly Regex Binnumber = new Regex("0[bB](?:_?[01])+");
	    private static readonly Regex Octnumber = new Regex("0[oO](?:_?[0-7])+");
	    private static readonly Regex Decnumber = new Regex("(?:0(?:_?0)*|[1-9](?:_?[0-9])*)");
	    private static readonly Regex Intnumber = new Regex(Group(Hexnumber, Binnumber, Octnumber, Decnumber));
	    private static readonly Regex Exponent = new Regex("[eE][-+]?[0-9](?:_?[0-9])*");
	    private static readonly Regex Pointfloat = new Regex(Group("[0-9](?:_?[0-9])*\\.(?:[0-9](?:_?[0-9])*)?", "\\.[0-9](?:_?[0-9])*" + Maybe(Exponent)));
	    private static readonly Regex Expfloat = new Regex("[0-9](?:_?[0-9])*" + Exponent);
	    private static readonly Regex Floatnumber = new Regex(Group(Pointfloat, Expfloat));
	    private static readonly Regex Imagnumber = new Regex(Group("[0-9](?:_?[0-9])*[jJ]", Floatnumber + "[jJ]"));
	    private static readonly Regex Number = new Regex(Group(Imagnumber, Floatnumber, Intnumber));
	    */

		#endregion
	}
}
