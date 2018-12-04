using System;

namespace Axion.Core.Tokens {
    /// <summary>
    ///     Represents a &lt;string literal&gt; <see cref="Token" />.
    /// </summary>
    public class StringToken : Token, IClosingToken {
        internal readonly char                 Quote;
        internal readonly StringLiteralOptions Options;
        private           string               trailingQuotes;

        private string _unescapedValue;

        internal string UnescapedValue {
            get => _unescapedValue;
            set {
                _unescapedValue = value;
                RecomputeEndPosition();
            }
        }

        private bool _isUnclosed;

        public bool IsUnclosed {
            get => _isUnclosed;
            set {
                _isUnclosed = value;
                RecomputeEndPosition();
            }
        }

        public StringToken(
            (int, int)           startPosition,
            StringLiteralOptions options,
            char                 usedQuote,
            string               value,
            string               unescapedValue = null,
            bool                 isUnclosed     = false,
            string               trailingQuotes = ""
        ) : base(TokenType.StringLiteral, startPosition, value) {
            if (unescapedValue == null) {
                unescapedValue = value;
            }
            Options             = options;
            Quote               = usedQuote;
            _unescapedValue     = unescapedValue;
            _isUnclosed         = isUnclosed;
            this.trailingQuotes = trailingQuotes;

            RecomputeEndPosition();
        }

        internal void AddTrailingQuotes(string quotes) {
            if (quotes.Length > 0) {
                if (!IsUnclosed) {
                    throw new InvalidOperationException("Cannot append trailing quote to finished string.");
                }
                trailingQuotes += quotes;
                RecomputeEndPosition();
            }
        }

        private void RecomputeEndPosition() {
            EndColumn = StartColumn;
            EndLine   = StartLine;
            // addition of quotes length:
            // compute count of quotes on token end line:
            // Multiline:  6 quotes on 1  line,  (3 * 2);
            //             3 quotes on 2+ lines, (3 * 1);
            // One-line:   2 quotes on 1  line,  (1 * 2);
            //             1 quote  on 2+ lines, (1 * 1).
            string[] lines       = Value.Split(Spec.EndOfLines, StringSplitOptions.None);
            int      quotesCount = Options.QuotesCount;
            if (lines.Length == 1) {
                EndColumn += lines[lines.Length - 1].Length;
                // if 1 line: add 1 for each prefix letter
                if (Options.IsRaw) {
                    EndColumn++;
                }
                if (Options.IsFormatted) {
                    EndColumn++;
                }

                if (IsUnclosed) {
                    EndColumn += quotesCount;
                }
                else {
                    EndColumn += quotesCount * 2;
                }
            }
            else if (!IsUnclosed) {
                EndColumn += quotesCount;
            }
            if (lines.Length > 1) {
                EndColumn =  lines[lines.Length - 1].Length;
                EndLine   += lines.Length;
            }
        }

        public override string ToAxionCode() {
            var result = "";
            if (Options.IsFormatted) {
                result += "f";
            }
            if (Options.IsRaw) {
                result += "r";
            }
            int quotesCount = Options.QuotesCount;
            result += new string(Quote, quotesCount) + Value;
            if (!IsUnclosed) {
                result += new string(Quote, quotesCount);
            }
            else {
                result += trailingQuotes;
            }
            return result + Whitespaces;
        }
    }

    public class StringLiteralOptions {
        internal bool IsMultiline;

        internal readonly bool IsLineEndsNormalized;

        internal bool IsFormatted { get; private set; }

        internal bool IsRaw { get; private set; }

        public StringLiteralOptions(
            bool isMultiline          = false,
            bool isLineEndsNormalized = false,
            bool isFormatted          = false,
            bool isRaw                = false
        ) {
            IsMultiline          = isMultiline;
            IsLineEndsNormalized = isLineEndsNormalized;
            IsFormatted          = isFormatted;
            IsRaw                = isRaw;
        }

        public bool HasPrefixes => IsFormatted || IsRaw;

        public int QuotesCount => IsMultiline
                                      ? 3
                                      : 1;

        public void AppendPrefix(char c, out bool valid, out bool duplicated) {
            duplicated = false;
            valid      = true;
            if (c == 'f' || c == 'F') {
                if (IsFormatted) {
                    duplicated = true;
                }
                IsFormatted = true;
            }
            else if (c == 'r' || c == 'R') {
                if (IsRaw) {
                    duplicated = true;
                }
                IsRaw = true;
            }
            else {
                valid = false;
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