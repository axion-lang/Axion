using System;
using System.Diagnostics;
using System.Globalization;
using Axion.Core.Hierarchy;
using Axion.Core.Processing.Lexical.Tokens;
using Axion.Core.Processing.Syntactic.Expressions;
using Axion.Core.Processing.Syntactic.Expressions.Common;
using Newtonsoft.Json;

namespace Axion.Core.Processing.Errors {
    /// <summary>
    ///     Exception that occurs during invalid
    ///     Axion code processing.
    /// </summary>
    public class LanguageReport : Exception {
        public override string        Message    { get; }
        public override string        StackTrace { get; }
        public          BlameSeverity Severity   { get; }

        [JsonProperty]
        public CodeSpan ErrorSpan { get; }

        [JsonProperty]
        public Unit TargetUnit { get; }

        [JsonProperty]
        public string Time { get; } =
            DateTime.Now.ToString(CultureInfo.InvariantCulture);

        private LanguageReport(
            string        message,
            BlameSeverity severity,
            CodeSpan      span
        ) {
            Severity   = severity;
            Message    = message;
            ErrorSpan  = span;
            TargetUnit = span.Unit;
            StackTrace = new StackTrace(2).ToString();
        }

        public static void To(BlameType type, CodeSpan span) {
            var ex = new LanguageReport(type.Description, type.Severity, span);
            ex.TargetUnit.Blames.Add(ex);
        }

        public static void UnexpectedType(Type expectedType, Expr expr) {
            BlameType blameType;
            if (expectedType == typeof(AtomExpr)) {
                blameType = BlameType.ExpectedAtomExpr;
            }
            else if (expectedType == typeof(PostfixExpr)) {
                blameType = BlameType.ExpectedPostfixExpr;
            }
            else if (expectedType == typeof(PrefixExpr)) {
                blameType = BlameType.ExpectedPrefixExpr;
            }
            else if (expectedType == typeof(InfixExpr)) {
                blameType = BlameType.ExpectedInfixExpr;
            }
            else {
                throw new ArgumentException(
                    $"Cannot get blame type for {expectedType.Name}"
                );
            }

            var ex = new LanguageReport(
                blameType.Description,
                blameType.Severity,
                expr
            );
            expr.Unit.Blames.Add(ex);
        }

        public static void MismatchedBracket(Token bracket) {
            var matchingBracket = bracket.Type.GetMatchingBracket();
            var ex = new LanguageReport(
                $"`{bracket.Value}` has no matching `{matchingBracket.GetValue()}`",
                BlameSeverity.Error,
                bracket
            );
            bracket.Unit.Blames.Add(ex);
        }

        public static void UnexpectedSyntax(
            TokenType expected,
            CodeSpan  span
        ) {
            var ex = new LanguageReport(
                $"Invalid syntax, expected `{expected.GetValue()}`, got `{span.Unit.TokenStream.Peek.Type.GetValue()}`.",
                BlameSeverity.Error,
                span
            );
            span.Unit.Blames.Add(ex);
        }

        public override string ToString() {
            return $"{Severity}: {Message} ({ErrorSpan.Start})";
        }
    }
}
