namespace Axion.Tokens
{
	internal enum TokenType
	{
		Unknown,
		EOF,

		// Special characters
		OpenParenthese,
		CloseParenthese,
		OpenBracket,
		CloseBracket,
		OpenCurly,
		CloseCurly,
		Comma,
		Colon,
		Semicolon,
		Indent,
		Outdent,
		Newline,

		// Literals
		String,
		Number,
		Identifier,
		Keyword,
		BuiltInType,
		Operator
	}
}