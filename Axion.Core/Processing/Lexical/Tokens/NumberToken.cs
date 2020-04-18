using Axion.Core.Source;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class NumberToken : Token {
        // TODO: declare ValueType
        internal NumberToken(SourceUnit source, string value = "", string content = "") : base(
            source,
            TokenType.Number,
            value,
            content
        ) { }
    }
}
