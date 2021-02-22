namespace Axion.Core.Processing.Errors {
    public class BlameType {
        public string        Description { get; }
        public BlameSeverity Severity    { get; }

        internal BlameType(string description, BlameSeverity severity) {
            Description = description;
            Severity    = severity;
        }
        // TODO: replace blame severities with class hierarchy
        // @formatter:off
        public static readonly BlameType InvalidCharacter                         = new("unknown character",                                               BlameSeverity.Error);
        public static readonly BlameType UnclosedMultilineComment                 = new("multiline comment has no closing scope",                          BlameSeverity.Error);
        public static readonly BlameType UnclosedString                           = new("string has no matching ending quote",                             BlameSeverity.Error);
        public static readonly BlameType UnclosedCharacterLiteral                 = new("character literal has no matching ending quote",                  BlameSeverity.Error);
        public static readonly BlameType CharacterLiteralTooLong                  = new("character literal exceeds max allowed length",                    BlameSeverity.Error);
        public static readonly BlameType EmptyCharacterLiteral                    = new("character literal cannot be empty",                               BlameSeverity.Error);
        public static readonly BlameType InvalidEscapeSequence                    = new("invalid character escaped by '\\'",                               BlameSeverity.Error);
        public static readonly BlameType IllegalUnicodeCharacter                  = new("literal has illegal unicode character",                           BlameSeverity.Error);
        public static readonly BlameType InvalidXEscapeFormat                     = new("invalid '\\x'-escape format",                                     BlameSeverity.Error);
        public static readonly BlameType TruncatedEscapeSequence                  = new("truncated escape sequence",                                       BlameSeverity.Error);
        public static readonly BlameType InvalidNumberRadix                       = new("number's base must be a number in range [1..36]",                 BlameSeverity.Error);
        public static readonly BlameType InvalidDigit                             = new("expected a decimal digit",                                        BlameSeverity.Error);
        public static readonly BlameType ExpectedNumberValueAfterNumberBase       = new("expected number value after base specifier",                      BlameSeverity.Error);
        public static readonly BlameType DigitValueIsAboveNumberRadix             = new("this digit is above number base",                                 BlameSeverity.Error);
        public static readonly BlameType DuplicatedParameterInFunction            = new("duplicated parameter in function definition",                     BlameSeverity.Error);
        public static readonly BlameType DuplicatedNamedArgument                  = new("duplicated named argument",                                       BlameSeverity.Error);
        public static readonly BlameType ExpectedDefaultParameterValue            = new("expected a default parameter value",                              BlameSeverity.Error);
        public static readonly BlameType ExpectedScopeDeclaration                 = new("scope expected",                                                  BlameSeverity.Error);
        public static readonly BlameType InvalidSyntax                            = new("invalid syntax",                                                  BlameSeverity.Error);
        public static readonly BlameType InvalidIndexerExpression                 = new("invalid indexer format",                                          BlameSeverity.Error);
        public static readonly BlameType IndentationBasedScopeNotAllowed          = new("scope based on indentation is not allowed in this context",       BlameSeverity.Error);
        public static readonly BlameType CannotHaveMoreThan1ListParameter         = new("only 1 list parameter is allowed",                                BlameSeverity.Error);
        public static readonly BlameType NamedArgsMustFollowBareStar              = new("named arguments must follow bare *",                              BlameSeverity.Error);
        public static readonly BlameType InvalidStringPrefix                      = new("invalid string prefix",                                           BlameSeverity.Error);
        public static readonly BlameType ExpectedVarName                          = new("variable name expected",                                          BlameSeverity.Error);
        public static readonly BlameType ExpectedImportedModuleName               = new("imported module name expected",                                   BlameSeverity.Error);
        public static readonly BlameType ImpossibleToInferType                    = new("impossible to infer type in this context",                        BlameSeverity.Warning);
        public static readonly BlameType NameIsAlreadyDefined                     = new("this name is already defined above",                              BlameSeverity.Error);
        public static readonly BlameType InvalidMacroParameter                    = new("invalid parameter for macro",                                     BlameSeverity.Error);
        public static readonly BlameType ModuleSelfImport                         = new("can't import module from itself",                                 BlameSeverity.Error);
        public static readonly BlameType CodeQuoteOutsideMacroDef                 = new("code quotes are allowed only inside a macro definition",          BlameSeverity.Error);
        public static readonly BlameType ExpectedTypeParameter                    = new("type parameter (e.g. 'T <- Type1, Type2') definition expected",   BlameSeverity.Error);

        public static readonly BlameType ExpectedAtomExpr                         = new("atomic expression expected",                                      BlameSeverity.Error);
        public static readonly BlameType ExpectedPostfixExpr                      = new("postfix expression expected",                                     BlameSeverity.Error);
        public static readonly BlameType ExpectedPrefixExpr                       = new("prefix expression expected",                                      BlameSeverity.Error);
        public static readonly BlameType ExpectedInfixExpr                        = new("infix expression expected",                                       BlameSeverity.Error);

        public static readonly BlameType DuplicatedImport                         = new("module is already imported",                                      BlameSeverity.Warning);
        public static readonly BlameType Redundant10Radix                         = new("redundant specifier, number radix is 10 by default",              BlameSeverity.Warning);
        public static readonly BlameType RedundantEmptyListOfTypeArguments        = new("empty list of type arguments is redundant",                       BlameSeverity.Warning);
        
        public static readonly BlameType InconsistentIndentation                  = new("mixed indentation (spaces and tabs)",                             BlameSeverity.Info);
        public static readonly BlameType RedundantStringFormat                    = new("string has format prefix but does not have any interpolation",    BlameSeverity.Info);
        public static readonly BlameType RedundantPrefixesForEmptyString          = new("prefixes are redundant for empty string",                         BlameSeverity.Info);
        public static readonly BlameType RedundantParentheses                     = new("parentheses around single expression are not needed",             BlameSeverity.Info);
        // @formatter:on
    }
}
