using System.Collections.Generic;
using System.Web;
using Axion.Core.Processing.CodeGen;
using Axion.Core.Processing.Errors;
using Axion.Core.Processing.Syntactic.Expressions.TypeNames;
using Axion.Core.Source;
using Axion.Core.Specification;

namespace Axion.Core.Processing.Lexical.Tokens {
    public class StringToken : Token {
        public bool IsUnclosed { get; protected set; }
        public string Prefixes { get; }
        public string Quote { get; protected set; }
        public bool EolsNormalize { get; }
        public string EndingQuotes { get; protected set; }
        public List<StringInterpolation> Interpolations { get; }
        public bool IsMultiline => Quote.Length == 3;
        public override TypeName ValueType => Spec.StringType;

        public StringToken(
            SourceUnit                source,
            string                    value          = "",
            bool                      isUnclosed     = false,
            string                    prefixes       = "",
            string                    quote          = "\"",
            bool                      eolsNormalize  = false,
            List<StringInterpolation> interpolations = null,
            Location                  start          = default,
            Location                  end            = default
        ) : base(source, TokenType.String, value, start: start, end: end) {
            IsUnclosed     = isUnclosed;
            Prefixes       = prefixes;
            Quote          = quote;
            EolsNormalize  = eolsNormalize;
            Interpolations = interpolations ?? new List<StringInterpolation>();
            EndingQuotes   = "";
        }

        public bool HasPrefix(string prefix) {
            return Prefixes.Contains(prefix.ToLower())
                || Prefixes.Contains(prefix.ToUpper());
        }

        public override Token Read() {
            AppendNext(expected: Spec.StringQuotes);
            Quote = Value[0].ToString();
            if (AppendNext(expected: Quote.Multiply(2))) {
                Quote = Quote.Multiply(3);
            }
            else if (AppendNext(expected: Quote)) {
                if (Prefixes.Length > 0) {
                    LangException.Report(BlameType.RedundantPrefixesForEmptyString, this);
                }

                return this;
            }

            while (true) {
                if (Stream.PeekIs(Spec.EscapeMark) && !HasPrefix("r")) {
                    ReadEscapeSeq();
                }
                else if (Stream.PeekIs(Spec.EscapeMark) && !IsMultiline
                      || Stream.PeekIs(Spec.Eoc)) {
                    IsUnclosed = true;
                    LangException.Report(BlameType.UnclosedString, this);
                    break;
                }
                else if (Stream.Eat("\r\n") != null && EolsNormalize) {
                    // \r\n -> \n
                    Value   += "\n";
                    Content += "\n";
                }
                else if (AppendNext(expected: Quote[0].ToString())) {
                    string closingQuote = Quote[0].ToString();
                    if (Quote.Length == 1
                     || AppendNext(expected: closingQuote.Multiply(2))) {
                        break;
                    }

                    Location closingLoc = Stream.Location;
                    EndingQuotes += closingQuote;
                    if (AppendNext(expected: closingQuote)) {
                        EndingQuotes += closingQuote;
                    }

                    LangException.Report(
                        BlameType.UnescapedQuoteInStringLiteral,
                        new Span(Source, closingLoc, Stream.Location)
                    );
                    break;
                }
                else if (HasPrefix("f") && Stream.PeekIs("{")) {
                    Interpolations.Add(new StringInterpolation(Stream).Read());
                }
                else {
                    AppendNext(true);
                }
            }

            if (HasPrefix("f") && Interpolations.Count == 0) {
                LangException.Report(BlameType.RedundantStringFormat, this);
            }

            return this;
        }

        public override void ToCSharp(CodeWriter c) {
            if (HasPrefix("f")) {
                c.Write("$");
            }

            if (HasPrefix("r") || IsMultiline) {
                c.Write("@");
            }

            c.Write("\"", HttpUtility.JavaScriptStringEncode(Content));
            if (!IsUnclosed) {
                c.Write("\"");
            }
        }

        public override void ToPython(CodeWriter c) {
            if (HasPrefix("f")) {
                c.Write("f");
            }

            if (HasPrefix("r")) {
                c.Write("r");
            }

            c.Write(IsMultiline ? "\"\"\"" : "\"");
            c.Write(HttpUtility.JavaScriptStringEncode(Content));
            if (!IsUnclosed) {
                c.Write(IsMultiline ? "\"\"\"" : "\"");
            }
        }
    }

    public class StringInterpolation : Span {
        public StringInterpolation(TextStream stream)
            : base(SourceUnit.FromInterpolation(stream)) { }

        public StringInterpolation Read() {
            Source.ProcessTerminators.Add(TokenType.CloseBrace);
            Compiler.LexicalAnalysis(Source);
            Source.ProcessTerminators.Remove(TokenType.CloseBrace);
            // remove '{' '}'
            Source.TokenStream.Tokens.RemoveAt(0);
            Source.TokenStream.Tokens.RemoveAt(
                Source.TokenStream.Tokens.Count - 1
            );
            return this;
        }
    }
}