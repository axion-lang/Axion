using Axion.Core.Hierarchy;
using Axion.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CommentToken : Token {
        public bool IsMultiline { get; }
        public bool IsUnclosed  { get; }

        internal CommentToken(
            Unit   unit,
            string value       = "",
            string content     = "",
            bool   isMultiline = false,
            bool   isUnclosed  = false
        ) : base(
            unit,
            TokenType.Comment,
            value,
            content
        ) {
            IsUnclosed  = isUnclosed;
            IsMultiline = isMultiline;
        }
    }
}
