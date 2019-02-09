namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents an 'end of input stream' ('EOS') mark.
    /// </summary>
    public class EndOfCodeToken : Token {
        public EndOfCodeToken(Position startPosition) : base(TokenType.EndOfCode, startPosition) {
        }
    }
}