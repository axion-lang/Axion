using System;
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
            { InvalidOperator,                                                        Error },
            { InvalidCharacter,                                                       Error }, 
            { MismatchedParenthesis,                                                  Error }, 
            { MismatchedBracket,                                                      Error }, 
            { MismatchedBrace,                                                        Error }, 
            { UnclosedMultilineComment,                                               Error }, 
            { InvalidEscapeSequence,                                                  Error }, 
            { IllegalUnicodeCharacter,                                                Error }, 
            { InvalidXEscapeFormat,                                                   Error }, 
            { TruncatedEscapeSequence,                                                Error }, 
            { UnclosedString,                                                         Error }, 
            { InvalidPrefixInStringLiteral,                                           Error }, 
            { UnescapedQuoteInStringLiteral,                                          Error }, 
            { UnclosedCharacterLiteral,                                               Error }, 
            { CharacterLiteralTooLong,                                                Error }, 
            { EmptyCharacterLiteral,                                                  Error }, 
            { InvalidNumberLiteral,                                                   Error }, 
            { InvalidBinaryLiteral,                                                   Error }, 
            { InvalidOctalLiteral,                                                    Error }, 
            { InvalidHexadecimalLiteral,                                              Error }, 
            { InvalidPostfixInNumberLiteral,                                          Error }, 
            { RepeatedDotInNumberLiteral,                                             Error }, 
            { ExpectedNumberValueAfterNumberBaseSpecifier,                            Error }, 
            { ExpectedNumberAfterExponentSign,                                        Error }, 
            { ExpectedEndOfNumberAfterPostfix,                                        Error }, 
            { ExpectedABitRateAfterNumberPostfix,                                     Error }, 
            { InvalidIntegerNumberBitRate,                                            Error }, 
            { InvalidFloatNumberBitRate,                                              Error },
            { InvalidComplexNumberLiteral,                                            Error },
            { BreakIsOutsideLoop,                                                     Error }, 
            { ContinueIsOutsideLoop,                                                  Error }, 
            { ContinueNotSupportedInsideFinally,                                      Error }, 
            { MisplacedReturn,                                                        Error }, 
            { MisplacedYield,                                                         Error }, 
            { InvalidExpressionToDelete,                                              Error }, 
            { DuplicatedParameterNameInFunctionDefinition,                            Error },
            { DefaultCatchMustBeLast,                                                 Error }, 
            { UnexpectedEndOfCode,                                                    Error }, 
            { DuplicatedNamedArgument,                                                Error },
            { ExpectedDefaultParameterValue,                                          Error },
            { ExpectedBlockDeclaration,                                               Error }, 
            { ConstantValueExpected,                                                  Error },
            { RedundantColonWithBraces,                                               Error }, 
            { ModulesAreNotSupportedInInterpretationMode,                             Error },
            { InvalidDecoratorPlacement,                                              Error },
            { ThisExpressionTargetIsNotAssignable,                                    Error },
            { LambdaCannotHaveIndentedBody,                                           Error },
            { CannotRedeclareVariableAlreadyDeclaredInThisScope,                      Error },
            { EmptyCollectionLiteralNotSupported,                                     Error },
            { CannotHaveMoreThan1ListParameter,                                       Error },
            { InvalidIndexerExpression,                                               Error },
            { CollectionInitializerCannotContainItemsAfterComprehension,              Error },
            
            { InconsistentIndentation,                                                Warning },
            { DuplicatedStringPrefix,                                                 Warning },
            { RedundantStringFormatPrefix,                                            Warning },
            { RedundantPrefixesForEmptyString,                                        Warning },
            { RedundantExponentForZeroNumber,                                         Warning },
            { RedundantEmptyUseStatement,                                             Warning },
            { DoubleNegationIsMeaningless,                                            Warning },
            { RedundantEmptyListOfTypeArguments,                                      Warning }
        };

        internal static BlameType InvalidNumberLiteralError(int radix) {
            switch (radix) {
                case 2:
                    return InvalidBinaryLiteral;
                case 8:
                    return InvalidOctalLiteral;
                case 10:
                    return InvalidNumberLiteral;
                case 16:
                    return InvalidHexadecimalLiteral;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static string CannotInferTypeError(Type exprType) {
            return "Cannot infer type of "
                   + Utilities.GetExprFriendlyName(exprType.Name)
                   + " due to invalid context";
        }
    }
}