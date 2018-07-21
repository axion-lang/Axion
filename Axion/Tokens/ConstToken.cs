namespace Axion.Tokens {
    public class ConstToken : Token {
        public ConstToken(TokenType type, (int, int) position, object value)
            : base(type, position, value.ToString()) {
        }
    }
}