using Axion.Core.Processing.CodeGen;
using Axion.Core.Source;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CommentToken : Token {
        public bool IsMultiline { get; }
        public bool IsUnclosed  { get; }

        internal CommentToken(
            SourceUnit source,
            string     value       = "",
            string     content     = "",
            bool       isMultiline = false,
            bool       isUnclosed  = false
        ) : base(source, TokenType.Comment, value, content) {
            IsUnclosed  = isUnclosed;
            IsMultiline = isMultiline;
        }

        public override void ToAxion(CodeWriter c) {
            if (IsMultiline) {
                c.Write(MultiLineCommentMark + Content);
                if (!IsUnclosed) {
                    c.Write(MultiLineCommentMark);
                }
            }
            else {
                c.Write(OneLineCommentMark + Content);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            if (IsMultiline) {
                c.Write("/*" + Content);
                if (!IsUnclosed) {
                    c.Write("*/");
                }
            }
            else {
                c.Write("//" + Content);
            }
        }
    }
}