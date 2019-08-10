from __future__ import annotations

import specification as spec
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.text_location import Location, Span, span_marker
from source_unit import SourceUnit


class NumberToken(Token):
    def __init__(
            self,
            source: SourceUnit,
            value = '',
            radix = 10,
            start = Location(0, 0),
            end = Location(0, 0)
    ):
        super().__init__(source, TokenType.number, value, start = start, end = end)
        self.radix = radix

    @property
    def is_floating(self) -> bool:
        return '.' in self.value

    @span_marker
    def read(self) -> NumberToken:
        # leading 0s
        while self.append_next('0', content = True):
            pass
        radix_specifier = ''
        while self.append_next(*spec.number_start):
            radix_specifier += self.stream.c
        if self.append_next(spec.number_radix_delimiter):
            # region radix reading
            try:
                self.radix = int(radix_specifier)
                if self.radix < 1 or self.radix > 36:
                    raise ValueError
            except ValueError:
                self.source.blame(
                    BlameType.invalid_number_radix,
                    Span(self.source, self.start, self.stream.location)
                )
            else:
                if self.radix == 10:
                    self.source.blame(
                        BlameType.redundant_10_radix,
                        Span(self.source, self.start, self.stream.location)
                    )
            # endregion
            if not self.stream.peek_is(*spec.number_part):
                self.source.blame(
                    BlameType.expected_number_value_after_number_base,
                    self
                )
                return self
            while self.append_next(*spec.number_part):
                if self.stream.c.isalnum() and int(self.stream.c, self.radix) >= self.radix:
                    self.source.blame(BlameType.digit_value_is_above_number_radix, self)
        else:
            # if found no radix delimiter, then
            # radix_specifier is the decimal number.
            self.content += radix_specifier

        return self

    def read_value(self):
        pass

    def to_csharp(self, c: CodeBuilder):
        c += self.value
