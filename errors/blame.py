from aenum import Enum, AutoNumberEnum


class BlameSeverity(AutoNumberEnum):
    info = None
    warning = None
    error = None


class BlameType(Enum):
    # @formatter:off

    invalid_character                        = "Unknown character",                                                   BlameSeverity.error
    # mismatches
    mismatched_parenthesis                   = "'(' has no matching ''.",                                             BlameSeverity.error
    mismatched_bracket                       = "'[' has no matching ']'.",                                            BlameSeverity.error
    mismatched_brace                         = "'{' has no matching '}'.",                                            BlameSeverity.error
    mismatched_double_brace                  = "'{{' has no matching '}}'.",                                          BlameSeverity.error
    #
    unclosed_multiline_comment               = "Multiline comment has no closing block.",                             BlameSeverity.error
    unclosed_string                          = "String has no matching ending quote.",                                BlameSeverity.error
    # invalid_prefix_in_string_literal         = "String has invalid prefix letter.",                                   BlameSeverity.error
    unescaped_quote_in_string_literal        = "String has unescaped quote character.",                               BlameSeverity.error
    unclosed_character_literal               = "Character literal has no matching ending quote.",                     BlameSeverity.error
    character_literal_too_long               = "Character literal exceeds max allowed length.",                       BlameSeverity.error
    empty_character_literal                  = "Character literal cannot be empty.",                                  BlameSeverity.error
    invalid_escape_sequence                  = "Invalid character escaped by '\\'.",                                  BlameSeverity.error
    illegal_unicode_character                = "Literal has illegal unicode character.",                              BlameSeverity.error
    invalid_x_escape_format                  = "Invalid '\\x'-escape format.",                                        BlameSeverity.error
    truncated_escape_sequence                = "Truncated escape sequence.",                                          BlameSeverity.error
    invalid_number_radix                     = "Number's base must be a number in range from 1 to 36 inclusive.",     BlameSeverity.error
    redundant_10_radix                       = "Redundant specifier, number radix is 10 by default.",                 BlameSeverity.warning
    invalid_digit                            = "Expected a decimal digit.",                                           BlameSeverity.error
    expected_number_value_after_number_base  = "Expected number value after base specifier.",                         BlameSeverity.error
    digit_value_is_above_number_radix        = "This digit is above number base.",                                    BlameSeverity.error
    expected_simple_name                     = "Expected a simple name, but got qualified.",                          BlameSeverity.error
    # invalid_number_literal                   = "Invalid number format.",                                              BlameSeverity.error
    # invalid_binary_literal                   = "Invalid binary number format.",                                       BlameSeverity.error
    # invalid_octal_literal                    = "Invalid octal number format.",                                        BlameSeverity.error
    # invalid_hexadecimal_literal              = "Invalid hexadecimal number format.",                                  BlameSeverity.error
    # invalid_postfix_in_number_literal        = "Number has invalid postfix.",                                         BlameSeverity.error
    # repeated_dot_in_number_literal           = "Number has more than one point.",                                     BlameSeverity.error
    # expected_number_after_exponent_sign      = "Missing value after number exponent specifier.",                      BlameSeverity.error
    # expected_end_of_number_after_postfix     = "Value after number postfix is not allowed.",                          BlameSeverity.error
    # expected_a_bit_rate_after_number_postfix = "Number with 'i' postfix must be followed by bit rate.",               BlameSeverity.error
    # invalid_integer_number_bit_rate          = "Integer number has invalid bit rate.",                                BlameSeverity.error
    # invalid_float_number_bit_rate            = "Floating point number has invalid bit rate.",                         BlameSeverity.error
    # invalid_complex_number_literal           = "Complex number literal has invalid format.",                          BlameSeverity.error
    # unexpected_token                         = "There is no language expression that has this syntax.",               BlameSeverity.error
    # break_is_outside_loop                    = "'break' cannot be outside cyclic expression.",                        BlameSeverity.error
    # continue_is_outside_loop                 = "'continue' cannot be outside cyclic expression.",                     BlameSeverity.error
    # misplaced_return                         = "'return' is in invalid context.",                                     BlameSeverity.error
    duplicated_parameter_in_function         = "Duplicated parameter in function definition.",                        BlameSeverity.error
    # default_catch_must_be_last               = "Default 'catch' must be placed after all other 'catch' expressions.", BlameSeverity.error
    unexpected_end_of_code                   = "Unexpected end of code.",                                             BlameSeverity.error
    duplicated_named_argument                = "Duplicated named argument.",                                          BlameSeverity.error
    expected_default_parameter_value         = "Expected a default parameter value.",                                 BlameSeverity.error
    expected_block_declaration               = "Block expected.",                                                     BlameSeverity.error
    # constant_value_expected                  = "Constant expected.",                                                  BlameSeverity.error

    inconsistent_indentation                    = "Mixed indentation (spaces and tabs).",                          BlameSeverity.warning
    # duplicated_string_prefix                    = "String literal has prefix that repeated more than 1 time.",     BlameSeverity.warning
    redundant_string_format                     = "String has format prefix but does not have any interpolation.", BlameSeverity.warning
    redundant_prefixes_for_empty_string         = "Prefixes are redundant for empty string.",                      BlameSeverity.warning
    # redundant_exponent_for_0                    = "Exponent is meaningless for 0.",                                BlameSeverity.warning
    module_not_supported_in_interpretation_mode = "'module' is not supported in interpretation mode.",             BlameSeverity.warning
    # expression_is_not_assignable                = "Expression is not assignable target.",                          BlameSeverity.warning
    lambda_cannot_have_indented_body            = "'lambda' cannot have block specified by indentation.",          BlameSeverity.warning
    # cannot_redeclare_variable                   = "Variable with same name already declared in this scope.",       BlameSeverity.warning
    cannot_have_more_than_1_list_parameter      = "Only 1 list parameter is allowed.",                             BlameSeverity.warning
    invalid_indexer_expression                  = "Invalid indexer format.",                                       BlameSeverity.warning
    redundant_colon_with_braces                 = "':' is not needed when block is specified by braces.",          BlameSeverity.warning
    # redundant_empty_list_of_type_arguments      = "Empty list of type arguments is redundant.",                    BlameSeverity.warning

    # @formatter:on

    def __init__(self, description: str, severity: BlameSeverity):
        self.description = description
        self.severity = severity
