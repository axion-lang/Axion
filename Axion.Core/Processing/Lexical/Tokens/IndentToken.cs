namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'indentation increasing' mark.
    /// </summary>
    public class IndentToken : Token {
        public IndentToken(string value = "    ", Position startPosition = default) : base(
            TokenType.Indent,
            value,
            startPosition
        ) { }
    }
}