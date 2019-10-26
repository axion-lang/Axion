using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class CharToken : Token {
        public override TypeName ValueType => Spec.CharType;

        public bool IsUnclosed { get; private set; }

        public CharToken(
            SourceUnit source,
            string     value      = "",
            bool       isUnclosed = false,
            Location   start      = default,
            Location   end        = default
        ) : base(source, TokenType.Character, value, start: start, end: end) {
            IsUnclosed = isUnclosed;
        }

        public override Token Read() {
            AppendNext(expected: Spec.CharacterQuote);
            while (!AppendNext(expected: Spec.CharacterQuote)) {
                if (Stream.AtEndOfLine) {
                    LangException.Report(BlameType.UnclosedCharacterLiteral, this);
                    IsUnclosed = true;
                    return this;
                }

                if (Stream.PeekIs(Spec.EscapeMark)) {
                    ReadEscapeSeq();
                }
                else {
                    AppendNext(true);
                }
            }

            if (Content.Length == 0) {
                LangException.Report(BlameType.EmptyCharacterLiteral, this);
            }
            else if (Content.Replace("\\", "").Length != 1) {
                LangException.Report(BlameType.CharacterLiteralTooLong, this);
            }

            return this;
        }

        public override void ToAxion(CodeWriter c) {
            c.Write(Spec.CharacterQuote, Value);
            if (!IsUnclosed) {
                c.Write(Spec.CharacterQuote);
            }
        }

        public override void ToCSharp(CodeWriter c) {
            c.Write("'", Value);
            if (!IsUnclosed) {
                c.Write("'");
            }
        }

        public override void ToPython(CodeWriter c) {
            c.Write("'", Value);
            if (!IsUnclosed) {
                c.Write("'");
            }
        }
    }
}