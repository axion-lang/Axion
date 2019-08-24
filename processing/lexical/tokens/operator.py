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
            input_side = InputSide.unknown,
            start = Location(0, 0),
            end = Location(0, 0)
    ):
        super().__init__(source, ttype, value, start = start, end = end)
        if ttype != TokenType.unknown:
            self.value = next((k for k, v in spec.operators.items() if v[0] == ttype), value)
        elif value:
            self.ttype = spec.operators[value][0]
        else:
            self.precedence = -1
            self.input_side = InputSide.unknown
            return
        self.precedence = spec.operators[self.value][1] if precedence == -1 else precedence
        self.input_side = spec.operators[self.value][2] if input_side == InputSide.unknown else input_side

    @span_marker
    def read(self) -> OperatorToken:
        self.append_next(*spec.operators_keys, content = True)
        if self.value is not None:
            self.ttype, self.precedence, self.input_side = spec.operators[self.value]
        return self
