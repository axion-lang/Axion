using Axion.Core.Source;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CommentToken : Token {
        public bool IsMultiline { get; }
        public bool IsUnclosed  { get; }

        internal CommentToken(
            Unit   source,
            string value       = "",
            string content     = "",
            bool   isMultiline = false,
            bool   isUnclosed  = false
        ) : base(
            source,
            TokenType.Comment,
            value,
            content
        ) {
            IsUnclosed  = isUnclosed;
            IsMultiline = isMultiline;
        }
    }
}
