from __future__ import annotations

import re
from string import ascii_letters, digits, hexdigits
from typing import Dict, List

from processing.lexical.tokens.operator import InputSide
from processing.lexical.tokens.token_type import TokenType

eoc = '\0'
eols = ['\r', '\n']
white = [' ', '\t']

oneline_comment_mark = '#'
multiline_comment_mark = '###'

character_quote = '`'
string_quotes = ['\'', '\"']
string_prefixes = [
    'f', 'F',
    'r', 'R'
]

escape_mark = '\\'
escape_sequences = {
    '0':  '\u0000',
    'a':  '\u0007',
    'b':  '\u0008',
    'f':  '\u000c',
    'n':  '\u000a',
    'r':  '\u000d',
    't':  '\u0009',
    'v':  '\u000b',
    '\\': '\\',
    '"':  '"',
    '\'': '\'',
    '`':  '`'
}

number_hex = hexdigits
number_start = digits
number_part = digits + '_.' + ascii_letters
number_radix_delimiter = '::'

id_start = ascii_letters + '_'
id_not_end = '-'
id_after_not_end = id_start + digits
id_part = id_start + id_not_end + digits
id_end = id_start + '?!' + digits

operators: Dict[str, (TokenType, int, InputSide)] = {
    # @formatter:off
    'of':     (TokenType.op_of,                      17, InputSide.both),
    '.':      (TokenType.op_dot,                     17, InputSide.both),

    '++':     (TokenType.op_increment,               16, InputSide.unknown),
    '--':     (TokenType.op_decrement,               16, InputSide.unknown),

    '**':     (TokenType.op_power,                   15, InputSide.both),

    'not':    (TokenType.op_not,                     14, InputSide.right),
    '~':      (TokenType.op_bit_not,                 14, InputSide.right),

    '*':      (TokenType.op_multiply,                13, InputSide.both),
    '/':      (TokenType.op_true_divide,             13, InputSide.both),
    '//':     (TokenType.op_floor_divide,            13, InputSide.both),
    '%':      (TokenType.op_remainder,               13, InputSide.both),

    '+':      (TokenType.op_plus,                    12, InputSide.unknown),
    '-':      (TokenType.op_minus,                   12, InputSide.unknown),

    '<<':     (TokenType.op_bit_left_shift,          11, InputSide.both),
    '>>':     (TokenType.op_bit_right_shift,         11, InputSide.both),

    '<=>':    (TokenType.op_3way_compare,            10, InputSide.both),

    '<':      (TokenType.op_less,                    9, InputSide.both),
    '<=':     (TokenType.op_less_or_equal,           9, InputSide.both),
    '>':      (TokenType.op_greater,                 9, InputSide.both),
    '>=':     (TokenType.op_greater_or_equal,        9, InputSide.both),

    '==':     (TokenType.op_equals_equals,           8, InputSide.both),
    '!=':     (TokenType.op_not_equals,              8, InputSide.both),

    '&':      (TokenType.op_bit_and,                 7, InputSide.both),
    '^':      (TokenType.op_bit_xor,                 6, InputSide.both),
    '|':      (TokenType.op_bit_or,                  5, InputSide.both),

    # 'as':(TokenType.op_as, 4, InputSide.both),
    'is':     (TokenType.op_is,                      4, InputSide.both),
    'is-not': (TokenType.op_is_not,                  4, InputSide.both),
    'in':     (TokenType.op_in,                      4, InputSide.both),
    'not-in': (TokenType.op_not_in,                  4, InputSide.both),

    # infix function call here

    'and':    (TokenType.op_and,                     3, InputSide.both),
    '&&':     (TokenType.op_and,                     3, InputSide.both),

    'or':     (TokenType.op_or,                      2, InputSide.both),
    '||':     (TokenType.op_or,                      2, InputSide.both),

    '??':     (TokenType.op_2question,               1, InputSide.both),

    '=':      (TokenType.op_assign,                  0, InputSide.both),
    '+=':     (TokenType.op_plus_assign,             0, InputSide.both),
    '-=':     (TokenType.op_minus_assign,            0, InputSide.both),
    '**=':    (TokenType.op_power_assign,            0, InputSide.both),
    '*=':     (TokenType.op_multiply_assign,         0, InputSide.both),
    '/=':     (TokenType.op_floor_divide_assign,     0, InputSide.both),
    '//=':    (TokenType.op_true_divide_assign,      0, InputSide.both),
    '%=':     (TokenType.op_remainder_assign,        0, InputSide.both),
    '?=':     (TokenType.op_question_assign,         0, InputSide.both),
    '<<=':    (TokenType.op_bit_l_shift_assign,      0, InputSide.both),
    '>>=':    (TokenType.op_bit_r_shift_assign,      0, InputSide.both),
    '&=':     (TokenType.op_bit_and_assign,          0, InputSide.both),
    '|=':     (TokenType.op_bit_or_assign,           0, InputSide.both),
    '^=':     (TokenType.op_bit_xor_assign,          0, InputSide.both)
    # @formatter:on
}

operators_keys = sorted(list(operators.keys()), reverse = True)

operators_signs = [op for op in operators_keys if not op[0] in id_start]

punctuation: Dict[str, TokenType] = {
    "->": TokenType.right_arrow,
    "<-": TokenType.left_arrow,
    "|>": TokenType.right_pipeline,
    "<|": TokenType.left_pipeline,
    "=>": TokenType.right_fat_arrow,
    "@":  TokenType.at,
    "?":  TokenType.question,
    "::": TokenType.colon_colon,

    "(":  TokenType.open_parenthesis,
    ")":  TokenType.close_parenthesis,
    "[":  TokenType.open_bracket,
    "]":  TokenType.close_bracket,
    "{":  TokenType.open_brace,
    "}":  TokenType.close_brace,
    "{{": TokenType.open_double_brace,
    "}}": TokenType.close_double_brace,
    ",":  TokenType.comma,
    ":":  TokenType.colon,
    ";":  TokenType.semicolon
}

punctuation_keys = sorted(list(punctuation.keys()), reverse = True)

keywords: Dict[str, TokenType] = {
    tpl[0][8:]: tpl[1] for tpl in TokenType.__members__.items() if tpl[0].startswith('keyword_')
}

constants: List[TokenType] = [
    TokenType.string,
    TokenType.character,
    TokenType.number,
    TokenType.keyword_true,
    TokenType.keyword_false,
    TokenType.keyword_nil
]

prefix_operators: List[TokenType] = [
    TokenType.op_increment,
    TokenType.op_decrement,
    TokenType.op_plus,
    TokenType.op_minus,
    TokenType.op_not,
    TokenType.op_bit_not
]

never_expr_start_types: List[TokenType] = [
    TokenType.op_assign,
    TokenType.op_plus_assign,
    TokenType.op_minus_assign,
    TokenType.op_multiply_assign,
    TokenType.op_true_divide_assign,
    TokenType.op_remainder_assign,
    TokenType.op_bit_and_assign,
    TokenType.op_bit_or_assign,
    TokenType.op_bit_xor_assign,
    TokenType.op_bit_l_shift_assign,
    TokenType.op_bit_r_shift_assign,
    TokenType.op_power_assign,
    TokenType.op_floor_divide_assign,
    TokenType.outdent,
    TokenType.newline,
    TokenType.end,
    TokenType.semicolon,
    TokenType.close_brace,
    TokenType.close_bracket,
    TokenType.close_parenthesis,
    TokenType.comma,
    TokenType.keyword_for,
    TokenType.op_in,
    TokenType.keyword_if,
]
