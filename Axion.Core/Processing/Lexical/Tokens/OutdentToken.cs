namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'indentation decreasing' mark.
    /// </summary>
    public class OutdentToken : Token {
        public OutdentToken(Position startPosition = default) : base(
            TokenType.Outdent,
            startPosition: startPosition
        ) { }
    }
}