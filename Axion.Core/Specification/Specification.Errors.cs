using System.Collections.Generic;
using Axion.Core.Processing.Errors;

namespace Axion.Core.Specification {
    public static partial class Spec {
        internal const string ERR_InvalidStatement =
            "Only assignment, call, increment, decrement, await expression, and new object expressions can be used as a statement";

        public static readonly Dictionary<BlameType, BlameSeverity> Blames = new Dictionary<BlameType, BlameSeverity> {
            // Errors
            { BlameType.InvalidOperator, BlameSeverity.Error },
            { BlameType.InvalidSymbol, BlameSeverity.Error },
            { BlameType.MismatchedParenthesis, BlameSeverity.Error },
            { BlameType.MismatchedBracket, BlameSeverity.Error },
            { BlameType.MismatchedBrace, BlameSeverity.Error },
            { BlameType.UnclosedMultilineComment, BlameSeverity.Error },
            { BlameType.InvalidEscapeSequence, BlameSeverity.Error },
            { BlameType.IllegalUnicodeCharacter, BlameSeverity.Error },
            { BlameType.InvalidXEscapeFormat, BlameSeverity.Error },
            { BlameType.TruncatedEscapeSequence, BlameSeverity.Error },
            { BlameType.UnclosedString, BlameSeverity.Error },
            { BlameType.InvalidPrefixInStringLiteral, BlameSeverity.Error },
            { BlameType.UnescapedQuoteInStringLiteral, BlameSeverity.Error },
            { BlameType.UnclosedCharacterLiteral, BlameSeverity.Error },
            { BlameType.CharacterLiteralTooLong, BlameSeverity.Error },
            { BlameType.EmptyCharacterLiteral, BlameSeverity.Error },
            { BlameType.InvalidNumberLiteral, BlameSeverity.Error },
            { BlameType.InvalidBinaryLiteral, BlameSeverity.Error },
            { BlameType.InvalidOctalLiteral, BlameSeverity.Error },
            { BlameType.InvalidHexadecimalLiteral, BlameSeverity.Error },
            { BlameType.InvalidPostfixInNumberLiteral, BlameSeverity.Error },
            { BlameType.RepeatedDotInNumberLiteral, BlameSeverity.Error },
            { BlameType.ExpectedNumberValueAfterNumberBaseSpecifier, BlameSeverity.Error },
            { BlameType.ExpectedNumberAfterExponentSign, BlameSeverity.Error },
            { BlameType.ExpectedEndOfNumberAfterPostfix, BlameSeverity.Error },
            { BlameType.ExpectedABitRateAfterNumberPostfix, BlameSeverity.Error },
            { BlameType.InvalidIntegerNumberBitRate, BlameSeverity.Error },
            { BlameType.InvalidFloatNumberBitRate, BlameSeverity.Error },
            { BlameType.BadCharacterForIntegerValue, BlameSeverity.Error },
            { BlameType.InvalidComplexNumberLiteral, BlameSeverity.Error },
            { BlameType.ComplexLiteralTooLarge, BlameSeverity.Error },
            { BlameType.AsyncModifierIsInapplicableToThatStatement, BlameSeverity.Error },

            { BlameType.BreakIsOutsideLoop, BlameSeverity.Error },
            { BlameType.ContinueIsOutsideLoop, BlameSeverity.Error },
            { BlameType.ContinueNotSupportedInsideFinally, BlameSeverity.Error },
            { BlameType.MisplacedReturn, BlameSeverity.Error },
            { BlameType.MisplacedYield, BlameSeverity.Error },
            { BlameType.InvalidExpressionToDelete, BlameSeverity.Error },
            { BlameType.DuplicatedArgumentInFunctionDefinition, BlameSeverity.Error },
            { BlameType.ExpectedIndentation, BlameSeverity.Error },
            { BlameType.UnexpectedIndentation, BlameSeverity.Error },
            { BlameType.InvalidIndentation, BlameSeverity.Error },
            { BlameType.DefaultCatchMustBeLast, BlameSeverity.Error },
            { BlameType.UnexpectedEndOfCode, BlameSeverity.Error },
            { BlameType.DuplicatedKeywordArgument, BlameSeverity.Error },
            { BlameType.InvalidSyntax, BlameSeverity.Error },
            { BlameType.ExpectedDefaultParameterValue, BlameSeverity.Error },
            { BlameType.CannotUseAccessModifierOutsideClass, BlameSeverity.Error },
            { BlameType.ExpectedBlockDeclaration, BlameSeverity.Error },

            // Warnings
            { BlameType.InconsistentIndentation, BlameSeverity.Warning },
            { BlameType.DuplicatedStringPrefix, BlameSeverity.Warning },
            { BlameType.RedundantStringFormatPrefix, BlameSeverity.Warning },
            { BlameType.RedundantPrefixesForEmptyString, BlameSeverity.Warning },
            { BlameType.RedundantExponentForZeroNumber, BlameSeverity.Warning },

            { BlameType.RedundantEmptyUseStatement, BlameSeverity.Warning },
            { BlameType.DoubleNegationIsMeaningless, BlameSeverity.Warning },
            { BlameType.ColonIsNotNeededWithBraces, BlameSeverity.Warning }
        };
    }
}