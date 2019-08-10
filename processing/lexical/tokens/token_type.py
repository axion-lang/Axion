from __future__ import annotations

from aenum import AutoNumberEnum, unique


@unique
class TokenType(AutoNumberEnum):
    # @formatter:off

    empty   = None
    invalid = None
    unknown = None

    # region gen_operators

    op_bit_and          = None
    op_bit_not          = None
    op_bit_or           = None
    op_bit_xor          = None
    op_bit_left_shift   = None
    op_bit_right_shift  = None
    op_and              = None
    op_or               = None
    keyword_as          = None
    op_is               = None
    op_is_not           = None
    op_not              = None
    op_in               = None
    op_not_in           = None
    op_equals_equals    = None
    op_not_equals       = None
    op_greater          = None
    op_greater_or_equal = None
    op_less             = None
    op_less_or_equal    = None
    op_plus             = None
    op_minus            = None
    op_increment        = None
    op_decrement        = None
    op_multiply         = None
    op_power            = None
    op_true_divide      = None
    op_floor_divide     = None
    op_remainder        = None
    op_3way_compare     = None
    op_2question        = None

    # assignment marks
    op_bit_and_assign      = None
    op_bit_or_assign       = None
    op_bit_xor_assign      = None
    op_bit_l_shift_assign  = None
    op_bit_r_shift_assign  = None
    op_plus_assign         = None
    op_minus_assign        = None
    op_multiply_assign     = None
    op_remainder_assign    = None
    op_true_divide_assign  = None
    op_floor_divide_assign = None
    op_power_assign        = None
    op_question_assign     = None
    op_assign              = None

    # endregion

    # region gen_keywords

    keyword_await    = None
    keyword_break    = None
    keyword_class    = None
    keyword_continue = None
    keyword_else     = None
    keyword_elif     = None
    keyword_enum     = None
    keyword_false    = None
    keyword_fn       = None
    keyword_for      = None
    keyword_from     = None
    keyword_if       = None
    keyword_macro    = None
    keyword_module   = None
    keyword_nil      = None
    keyword_nobreak  = None
    keyword_object   = None
    keyword_pass     = None
    keyword_return   = None
    keyword_true     = None
    keyword_unless   = None
    keyword_let      = None
    keyword_while    = None
    keyword_yield    = None
    custom_keyword   = None

    # endregion

    # region gen_symbols

    question        = None
    right_fat_arrow = None
    left_pipeline   = None
    right_pipeline  = None
    at              = None
    dot             = None
    comma           = None
    semicolon       = None
    colon           = None
    colon_colon     = None

    # brackets
    open_brace         = None
    open_double_brace  = None
    open_bracket       = None
    open_parenthesis   = None
    close_brace        = None
    close_double_brace = None
    close_bracket      = None
    close_parenthesis  = None

    # endregion

    # literals
    identifier = None
    comment    = None
    character  = None
    string     = None
    number     = None

    # white
    whitespace = None
    newline    = None
    indent     = None
    outdent    = None
    end        = None

    # @formatter:on

    @property
    def is_open_bracket(self) -> bool:
        return self in [
            TokenType.open_brace,
            TokenType.open_double_brace,
            TokenType.open_bracket,
            TokenType.open_parenthesis
        ]

    @property
    def is_close_bracket(self) -> bool:
        return self in [
            TokenType.close_brace,
            TokenType.close_double_brace,
            TokenType.close_bracket,
            TokenType.close_parenthesis
        ]

    @property
    def matching_bracket(self) -> TokenType:
        return {
            # open:                       close
            TokenType.open_brace:         TokenType.close_brace,
            TokenType.open_double_brace:  TokenType.close_double_brace,
            TokenType.open_bracket:       TokenType.close_bracket,
            TokenType.open_parenthesis:   TokenType.close_parenthesis,
            # close:                      open
            TokenType.close_brace:        TokenType.open_brace,
            TokenType.close_double_brace: TokenType.open_double_brace,
            TokenType.close_bracket:      TokenType.open_bracket,
            TokenType.close_parenthesis:  TokenType.open_parenthesis
        }.get(self)
