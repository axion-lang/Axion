using System.Collections.Generic;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    /// <summary>
    ///     Represents a 'string' literal.
    /// </summary>
    public class StringToken : Token {
        public string               EscapedValue   { get; }
        public bool                 IsUnclosed     { get; }
        public string               TrailingQuotes { get; }
        public StringLiteralOptions Options        { get; }
        public List<Interpolation>  Interpolations { get; }

        public StringToken(
            StringLiteralOptions options,
            string               value,
            string?              escapedValue   = null,
            List<Interpolation>? interpolations = null,
            bool                 isUnclosed     = false,
            string?              trailingQuotes = null,
            Position             startPosition  = default
        ) : base(TokenType.String, value) {
            Options        = options;
            Interpolations = interpolations ?? new List<Interpolation>();
            EscapedValue   = escapedValue ?? value;
            IsUnclosed     = isUnclosed;
            TrailingQuotes = trailingQuotes ?? "";

            // compute position
            string[] lines = Value.Split('\n');

            int endLine =
                startPosition.Line
                + (lines.Length > 1
                    ? lines.Length - 1
                    : 0);
            int endCol =
                lines[lines.Length - 1].Length
                + (lines.Length == 1
                    ? startPosition.Column
                      + Options.GetPrefixes().Length
                      + (IsUnclosed
                          ? Options.QuotesCount + TrailingQuotes.Length
                          : Options.QuotesCount * 2)
                    : IsUnclosed
                        ? TrailingQuotes.Length
                        : Options.QuotesCount);

            Span = new Span(startPosition, (endLine, endCol));
        }

        public override void ToOriginalAxionCode(CodeBuilder c) {
            ToAxionCode(c);
            c.Write(EndWhitespaces);
        }

        public override void ToAxionCode(CodeBuilder c) {
            c.Write(Options.GetPrefixes());
            int quotesCount = Options.QuotesCount;
            c.Write(new string(Options.Quote, quotesCount) + Value);
            if (IsUnclosed) {
                c.Write(TrailingQuotes);
            }
            else {
                c.Write(new string(Options.Quote, quotesCount));
            }
        }

        public override void ToCSharpCode(CodeBuilder c) {
            if (Options.IsFormatted) {
                c.Write("$");
            }

            if (Options.IsRaw || Options.QuotesCount == 3) {
                c.Write("@");
            }

            c.Write("\"");
            c.Write(Value);
            if (!IsUnclosed) {
                c.Write("\"");
            }
        }
    }

    public class Interpolation {
        public readonly List<Token> Tokens = new List<Token>();
        public readonly int         StartIndex;
        public          int         EndIndex;

        public Interpolation(int startIndex) {
            StartIndex = startIndex;
        }

        internal int Length => EndIndex - StartIndex;
    }

    public class StringLiteralOptions {
        public readonly bool IsLineEndsNormalized;
        public          bool IsMultiline;
        public          char Quote;
        public          bool HasPrefixes => IsFormatted || IsRaw;
        public          int  QuotesCount => IsMultiline ? 3 : 1;

        #region Prefix options

        public bool IsFormatted { get; private set; }
        public bool IsRaw       { get; private set; }

        #endregion

        public StringLiteralOptions(
            char quote                = '"',
            bool isMultiline          = false,
            bool isLineEndsNormalized = false
        ) {
            Quote                = quote;
            IsMultiline          = isMultiline;
            IsLineEndsNormalized = isLineEndsNormalized;
        }

        public void AppendPrefix(char c, out bool valid, out bool duplicated) {
            duplicated = false;
            valid      = true;
            switch (c) {
                case 'f':
                case 'F': {
                    if (IsFormatted) {
                        duplicated = true;
                    }

                    IsFormatted = true;
                    break;
                }

                case 'r':
                case 'R': {
                    if (IsRaw) {
                        duplicated = true;
                    }

                    IsRaw = true;
                    break;
                }

                default:
                    valid = false;
                    break;
            }
        }

        public string GetPrefixes() {
            var result = "";
            if (IsFormatted) {
                result += "f";
            }

            if (IsRaw) {
                result += "r";
            }

            return result;
        }
    }
}