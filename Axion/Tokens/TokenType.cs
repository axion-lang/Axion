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

		// Numbers
		Number_Float,
		Number_LFloat,
		Number_Int,
		Number_SInt,
		Number_LInt,
		Number_Byte,

		//
		String,
		Identifier,
		BuiltInType,
		Operator,
		Reference,
		Keyword
	}
}