using System;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'character' literal.
    /// </summary>
    public class CharacterToken : Token {
        public string EscapedValue { get; }
        public bool   IsUnclosed   { get; }

        public CharacterToken(string value) : base(TokenType.Character, value, default) {
            if (Value.Length > 1) {
                throw new Exception("Cannot create character literal with length > 1.");
            }

            EscapedValue = value;
        }

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

        internal override CodeBuilder ToAxionCode(CodeBuilder c) {
            c = c + Spec.CharacterLiteralQuote.ToString() + Value;
            if (!IsUnclosed) {
                c += Spec.CharacterLiteralQuote.ToString();
            }

            return c + EndWhitespaces;
        }

        internal override CodeBuilder ToCSharpCode(CodeBuilder c) {
            c = c + "'" + Value;
            if (!IsUnclosed) {
                c += "'";
            }

            return c + EndWhitespaces;
        }
    }
}