from aenum import Enum, AutoNumberEnum


class BlameSeverity(AutoNumberEnum):
    info = None
    warning = None
    error = None


class BlameType(Enum):
    # @formatter:off

    invalid_character                        = "unknown character",                                                  BlameSeverity.error
    # mismatches
    mismatched_parenthesis                   = "'(' has no matching ''",                                             BlameSeverity.error
    mismatched_bracket                       = "'[' has no matching ']'",                                            BlameSeverity.error
    mismatched_brace                         = "'{' has no matching '}'",                                            BlameSeverity.error
    mismatched_double_brace                  = "'{{' has no matching '}}'",                                          BlameSeverity.error
    #
    unclosed_multiline_comment               = "multiline comment has no closing block",                             BlameSeverity.error
    unclosed_string                          = "string has no matching ending quote",                                BlameSeverity.error
    unescaped_quote_in_string_literal        = "string has unescaped quote character",                               BlameSeverity.error
    unclosed_character_literal               = "character literal has no matching ending quote",                     BlameSeverity.error
    character_literal_too_long               = "character literal exceeds max allowed length",                       BlameSeverity.error
    empty_character_literal                  = "character literal cannot be empty",                                  BlameSeverity.error
    invalid_escape_sequence                  = "invalid character escaped by '\\'",                                  BlameSeverity.error
    illegal_unicode_character                = "literal has illegal unicode character",                              BlameSeverity.error
    invalid_x_escape_format                  = "invalid '\\x'-escape format",                                        BlameSeverity.error
    truncated_escape_sequence                = "truncated escape sequence",                                          BlameSeverity.error
    invalid_number_radix                     = "number's base must be a number in range from 1 to 36 inclusive",     BlameSeverity.error
    invalid_digit                            = "expected a decimal digit",                                           BlameSeverity.error
    expected_number_value_after_number_base  = "expected number value after base specifier",                         BlameSeverity.error
    digit_value_is_above_number_radix        = "this digit is above number base",                                    BlameSeverity.error
    expected_simple_name                     = "expected a simple name, but got qualified",                          BlameSeverity.error
    duplicated_parameter_in_function         = "duplicated parameter in function definition",                        BlameSeverity.error
    unexpected_end_of_code                   = "unexpected end of code",                                             BlameSeverity.error
    duplicated_named_argument                = "duplicated named argument",                                          BlameSeverity.error
    expected_default_parameter_value         = "expected a default parameter value",                                 BlameSeverity.error
    expected_block_declaration               = "block expected",                                                     BlameSeverity.error
    impossible_to_infer_type                 = "impossible to infer type in this context",                           BlameSeverity.error
    invalid_type_annotation                  = "invalid type annotation",                                            BlameSeverity.error
    invalid_syntax                           = "invalid syntax",                                                     BlameSeverity.error
    invalid_indexer_expression               = "invalid indexer format",                                             BlameSeverity.error
    lambda_cannot_have_indented_body         = "'lambda' cannot have block specified by indentation",                BlameSeverity.error
    cannot_have_more_than_1_list_parameter   = "only 1 list parameter is allowed",                                   BlameSeverity.error

    redundant_10_radix                          = "redundant specifier, number radix is 10 by default",           BlameSeverity.warning
    inconsistent_indentation                    = "mixed indentation (spaces and tabs)",                          BlameSeverity.warning
    redundant_string_format                     = "string has format prefix but does not have any interpolation", BlameSeverity.warning
    redundant_prefixes_for_empty_string         = "prefixes are redundant for empty string",                      BlameSeverity.warning
    module_not_supported_in_interpretation_mode = "'module' is not supported in interpretation mode",             BlameSeverity.warning
    redundant_colon_with_braces                 = "':' is not needed when block is specified by braces",          BlameSeverity.warning

    # @formatter:on

    def __init__(self, description: str, severity: BlameSeverity):
        self.description = description
        self.severity = severity
