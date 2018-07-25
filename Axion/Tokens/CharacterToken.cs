namespace Axion.Tokens {
    /// <summary>
    ///     Represents a character literal <see cref="Token" />.
    /// </summary>
    public class CharacterToken : Token {
        public CharacterToken((int line, int column) location, string value)
            : base(TokenType.CharLiteral, location, value) {
            EndClPos += 2; // ' & '
        }
    }
}