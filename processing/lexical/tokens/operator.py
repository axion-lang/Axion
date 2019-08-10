from __future__ import annotations

from aenum import AutoNumberEnum

import specification as spec
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.text_location import Location, span_marker
from source_unit import SourceUnit


class InputSide(AutoNumberEnum):
    unknown = None
    both = None
    left = None
    right = None


class OperatorToken(Token):
    def __init__(
            self,
            source: SourceUnit,
            value = '',
            ttype = TokenType.unknown,
            precedence = -1,
            operator = InputSide.unknown,
            start = Location(0, 0),
            end = Location(0, 0)
    ):
        super().__init__(source, ttype, value, start = start, end = end)
        if ttype != TokenType.unknown:
            self.ttype = list(filter(lambda t: t[0] == ttype, spec.operators.keys()))[0]
        elif value:
            self.ttype = spec.operators[value][0]
        else:
            self.precedence = -1
            self.operator = InputSide.unknown
            return
        self.precedence = spec.operators[self.value][1] if precedence == -1 else precedence
        self.operator = spec.operators[self.value][2] if operator == InputSide.unknown else operator

    @span_marker
    def read(self) -> OperatorToken:
        self.append_next(*spec.operators_keys, content = True)
        if self.value is not None:
            self.ttype, self.precedence, self.operator = spec.operators[self.value]
        return self
