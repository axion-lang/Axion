namespace Axion.Core.Processing.Errors {
    public class BlameType {
        public readonly string        Description;
        public readonly BlameSeverity Severity;

        internal BlameType(string description, BlameSeverity severity) {
            Description = description;
            Severity    = severity;
        }

        // @formatter:off
        public static readonly BlameType InvalidCharacter                         = new BlameType("unknown character",                                               BlameSeverity.Error);
        public static readonly BlameType MismatchedBracket                        = new BlameType("'[' has no matching ']'",                                         BlameSeverity.Error);
        public static readonly BlameType UnclosedMultilineComment                 = new BlameType("multiline comment has no closing block",                          BlameSeverity.Error);
        public static readonly BlameType UnclosedString                           = new BlameType("string has no matching ending quote",                             BlameSeverity.Error);
        public static readonly BlameType UnclosedCharacterLiteral                 = new BlameType("character literal has no matching ending quote",                  BlameSeverity.Error);
        public static readonly BlameType CharacterLiteralTooLong                  = new BlameType("character literal exceeds max allowed length",                    BlameSeverity.Error);
        public static readonly BlameType EmptyCharacterLiteral                    = new BlameType("character literal cannot be empty",                               BlameSeverity.Error);
        public static readonly BlameType InvalidEscapeSequence                    = new BlameType("invalid character escaped by '\\'",                               BlameSeverity.Error);
        public static readonly BlameType IllegalUnicodeCharacter                  = new BlameType("literal has illegal unicode character",                           BlameSeverity.Error);
        public static readonly BlameType InvalidXEscapeFormat                     = new BlameType("invalid '\\x'-escape format",                                     BlameSeverity.Error);
        public static readonly BlameType TruncatedEscapeSequence                  = new BlameType("truncated escape sequence",                                       BlameSeverity.Error);
        public static readonly BlameType InvalidNumberRadix                       = new BlameType("number's base must be a number in range [1..36]",                 BlameSeverity.Error);
        public static readonly BlameType InvalidDigit                             = new BlameType("expected a decimal digit",                                        BlameSeverity.Error);
        public static readonly BlameType ExpectedNumberValueAfterNumberBase       = new BlameType("expected number value after base specifier",                      BlameSeverity.Error);
        public static readonly BlameType DigitValueIsAboveNumberRadix             = new BlameType("this digit is above number base",                                 BlameSeverity.Error);
        public static readonly BlameType DuplicatedParameterInFunction            = new BlameType("duplicated parameter in function definition",                     BlameSeverity.Error);
        public static readonly BlameType DuplicatedNamedArgument                  = new BlameType("duplicated named argument",                                       BlameSeverity.Error);
        public static readonly BlameType ExpectedDefaultParameterValue            = new BlameType("expected a default parameter value",                              BlameSeverity.Error);
        public static readonly BlameType ExpectedBlockDeclaration                 = new BlameType("block expected",                                                  BlameSeverity.Error);
        public static readonly BlameType InvalidSyntax                            = new BlameType("invalid syntax",                                                  BlameSeverity.Error);
        public static readonly BlameType InvalidIndexerExpression                 = new BlameType("invalid indexer format",                                          BlameSeverity.Error);
        public static readonly BlameType IndentationBasedBlockNotAllowed          = new BlameType("block based on indentation is not allowed in this context",       BlameSeverity.Error);
        public static readonly BlameType CannotHaveMoreThan1ListParameter         = new BlameType("only 1 list parameter is allowed",                                BlameSeverity.Error);
        public static readonly BlameType NamedArgsMustFollowBareStar              = new BlameType("named arguments must follow bare *",                              BlameSeverity.Error);
        public static readonly BlameType InvalidStringPrefix                      = new BlameType("invalid string prefix",                                           BlameSeverity.Error);
        public static readonly BlameType ExpectedVarName                          = new BlameType("variable name expected",                                          BlameSeverity.Error);
        public static readonly BlameType ImpossibleToInferType                    = new BlameType("impossible to infer type in this context",                        BlameSeverity.Error);
        public static readonly BlameType NameIsAlreadyDefined                     = new BlameType("this name is already defined above",                              BlameSeverity.Error);
        
        public static readonly BlameType Redundant10Radix                         = new BlameType("redundant specifier, number radix is 10 by default",              BlameSeverity.Warning);
        public static readonly BlameType RedundantEmptyListOfTypeArguments        = new BlameType("empty list of type arguments is redundant",                       BlameSeverity.Warning);
        
        public static readonly BlameType InconsistentIndentation                  = new BlameType("mixed indentation (spaces and tabs)",                             BlameSeverity.Info);
        public static readonly BlameType RedundantStringFormat                    = new BlameType("string has format prefix but does not have any interpolation",    BlameSeverity.Info);
        public static readonly BlameType RedundantPrefixesForEmptyString          = new BlameType("prefixes are redundant for empty string",                         BlameSeverity.Info);
        public static readonly BlameType RedundantColonWithBraces                 = new BlameType("':' is not needed when block is specified by braces",             BlameSeverity.Info);
        // @formatter:on
    }
}