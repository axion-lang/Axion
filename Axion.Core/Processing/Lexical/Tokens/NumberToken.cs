using Axion.Core.Hierarchy;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class NumberToken : Token {
        // TODO: declare ValueType
        internal NumberToken(Unit unit, string value = "", string content = "")
            : base(
                unit,
                TokenType.Number,
                value,
                content
            ) { }
    }
}
