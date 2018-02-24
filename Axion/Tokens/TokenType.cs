namespace Axion.Tokens
{
    internal enum TokenType
    {
        Unknown,

        // Special operators:
        OpenParenthese,
        CloseParenthese,
        Comma,
        Semicolon,
        Indent,
        Outdent,
        Newline,

        // Types:
        Number_Float,
        Number_SFloat,
        Number_LFloat,
        Number_Int,
        Number_SInt,
        Number_LInt,
        Number_Byte,
        String,
        Identifier,

        // Other parts:
        Operator,
        Reference,
        Keyword
    }
}
