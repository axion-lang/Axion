using System.Collections.Generic;
using Axion.Core.Processing.Errors;
using static Axion.Core.Processing.Errors.BlameSeverity;
using static Axion.Core.Processing.Errors.BlameType;

namespace Axion.Core.Specification {
    public static partial class Spec {
        /// <summary>
        ///     Map, declaring severity
        ///     for all specified code blames.
        /// </summary>
        public static readonly Dictionary<BlameType, BlameSeverity> Blames = new Dictionary<BlameType, BlameSeverity> {
            // Errors
            { InvalidOperator, Error },
            { InvalidSymbol, Error },
            { MismatchedParenthesis, Error },
            { MismatchedBracket, Error },
            { MismatchedBrace, Error },
            { UnclosedMultilineComment, Error },
            { InvalidEscapeSequence, Error },
            { IllegalUnicodeCharacter, Error },
            { InvalidXEscapeFormat, Error },
            { TruncatedEscapeSequence, Error },
            { UnclosedString, Error },
            { InvalidPrefixInStringLiteral, Error },
            { UnescapedQuoteInStringLiteral, Error },
            { UnclosedCharacterLiteral, Error },
            { CharacterLiteralTooLong, Error },
            { EmptyCharacterLiteral, Error },
            { InvalidNumberLiteral, Error },
            { InvalidBinaryLiteral, Error },
            { InvalidOctalLiteral, Error },
            { InvalidHexadecimalLiteral, Error },
            { InvalidPostfixInNumberLiteral, Error },
            { RepeatedDotInNumberLiteral, Error },
            { ExpectedNumberValueAfterNumberBaseSpecifier, Error },
            { ExpectedNumberAfterExponentSign, Error },
            { ExpectedEndOfNumberAfterPostfix, Error },
            { ExpectedABitRateAfterNumberPostfix, Error },
            { InvalidIntegerNumberBitRate, Error },
            { InvalidFloatNumberBitRate, Error },
            { BadCharacterForIntegerValue, Error },
            { InvalidComplexNumberLiteral, Error },
            { ComplexLiteralTooLarge, Error },
            { AsyncModifierIsInapplicableToThatStatement, Error },
            { BreakIsOutsideLoop, Error },
            { ContinueIsOutsideLoop, Error },
            { ContinueNotSupportedInsideFinally, Error },
            { MisplacedReturn, Error },
            { MisplacedYield, Error },
            { InvalidExpressionToDelete, Error },
            { DuplicatedParameterNameInFunctionDefinition, Error },
            { ExpectedIndentation, Error },
            { UnexpectedIndentation, Error },
            { InvalidIndentation, Error },
            { DefaultCatchMustBeLast, Error },
            { UnexpectedEndOfCode, Error },
            { DuplicatedKeywordArgument, Error },
            { InvalidSyntax, Error },
            { ExpectedDefaultParameterValue, Error },
            { CannotUseAccessModifierOutsideClass, Error },
            { ExpectedBlockDeclaration, Error },
            { ConstantValueExpected, Error },
            { InvalidTypeNameExpression, Error },

            // Warnings
            { InconsistentIndentation, Warning },
            { DuplicatedStringPrefix, Warning },
            { RedundantStringFormatPrefix, Warning },
            { RedundantPrefixesForEmptyString, Warning },
            { RedundantExponentForZeroNumber, Warning },
            { RedundantEmptyUseStatement, Warning },
            { DoubleNegationIsMeaningless, Warning },
            { RedundantColonWithBraces, Warning },
            { RedundantEmptyListOfTypeArguments, Warning }
        };

        internal const string ERR_PrimaryExpected = "Expected an identifier, list, map or other primary expression.";

        internal const string ERR_InvalidStatement =
            "Only assignment, call, increment, decrement, await expression, and new object expressions can be used as a statement";

        internal const string ERR_InvalidDecoratorPosition =
            "Decorator can be applied only to the top level definition.";
    }
}