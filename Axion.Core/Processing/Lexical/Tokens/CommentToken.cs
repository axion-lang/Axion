using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Source;
using static Axion.Core.Specification.Spec;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CommentToken : Token {
        public bool IsMultiline { get; private set; }
        public bool IsUnclosed { get; private set; }

        public CommentToken(
            SourceUnit source,
            string     value       = "",
            bool       isMultiline = false,
            bool       isUnclosed  = false,
            Location   start       = default,
            Location   end         = default
        ) : base(source, TokenType.Comment, value, start: start, end: end) {
            IsUnclosed  = isUnclosed;
            IsMultiline = isMultiline;
        }

        public CommentToken ReadOneLine() {
            IsMultiline = false;
            AppendNext(expected: OneLineCommentMark);
            while (!Stream.AtEndOfLine) {
                AppendNext(true);
            }

            return this;
        }

        public CommentToken ReadMultiLine() {
            IsMultiline = true;
            AppendNext(expected: MultiLineCommentMark);
            while (!Stream.PeekIs(MultiLineCommentMark)) {
                if (Stream.PeekIs(Eoc)) {
                    LangException.Report(BlameType.UnclosedMultilineComment, this);
                    IsUnclosed = true;
                    return this;
                }

                AppendNext(true);
            }

            AppendNext(expected: MultiLineCommentMark);
            return this;
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