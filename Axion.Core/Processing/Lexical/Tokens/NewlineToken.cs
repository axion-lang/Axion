namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'newline' ('\n' or '\r\n') mark.
    /// </summary>
    public class NewlineToken : Token {
        public NewlineToken(string value = "\n", Position startPosition = default)
            : base(
                TokenType.Newline,
                value,
                startPosition
            ) { }
    }
}