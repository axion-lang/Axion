using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Syntactic.TypeNames;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'character' literal.
    /// </summary>
    public class CharacterToken : Token {
        public string EscapedValue { get; }
        public bool IsUnclosed { get; }
        public override TypeName ValueType => Spec.CharType;

        internal CharacterToken(
            string   value,
            string   escapedValue  = null,
            bool     isUnclosed    = false,
            Position startPosition = default
        ) : base(TokenType.Character, value) {
            EscapedValue = escapedValue ?? value;
            IsUnclosed   = isUnclosed;

            // compute position
            int endCol = startPosition.Column
                       + Value.Length
                       + (IsUnclosed ? 1 : 2); // quotes length
            Span = new Span(startPosition, (startPosition.Line, endCol));
        }

        internal override void ToAxionCode(CodeBuilder c) {
            c.Write(Spec.CharQuotes[0], Value);
            if (!IsUnclosed) {
                c.Write(Spec.CharQuotes[0]);
            }
        }

        internal override void ToCSharpCode(CodeBuilder c) {
            c.Write("'", Value);
            if (!IsUnclosed) {
                c.Write("'");
            }
        }
    }
}