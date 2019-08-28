import re
from typing import Optional

import specification as spec
from processing.location import Span, Location
from source import SourceUnit


class TextStream:
    def __init__(self, text: str = None):
        self.char_idx = -1

        self.line_idx = 0
        self.column_idx = 0

        self.prev_line_len = 0

        self.text = text
        if not self.text.endswith(spec.eoc):
            self.text += spec.eoc

    @property
    def location(self) -> Location:
        return Location(self.line_idx, self.column_idx)

    @property
    def at_eol(self) -> bool:
        return self.peek() in spec.eols or self.peek_is(spec.eoc)

    @property
    def rest_of_line(self) -> str:
        text_from_current = self.text[self.char_idx + 1:]
        try:
            return text_from_current[0:text_from_current.index('\n') + 1]
        except ValueError:
            return text_from_current

    @property
    def c(self) -> str:
        """
        :return: Character at current position.
        """
        if self.char_idx < 0:
            return spec.eoc
        return self.text[self.char_idx]

    def peek(self, length: int = 1) -> str:
        """
        Peeks next piece of text by specified length.

        :param length: Length to peek by.
        """
        if 0 <= (self.char_idx + 1 + length) < len(self.text):
            return self.text[self.char_idx + 1:self.char_idx + 1 + length]
        return spec.eoc

    def peek_is(self, *expected: str) -> bool:
        assert len(expected) > 0
        return any(value == self.peek(len(value)) for value in expected)

    # noinspection PyUnresolvedReferences
    def peek_match(self, regex: str) -> re.Match:
        txt = self.text[self.char_idx:]
        return re.match(regex, txt)

    def eat(self, *expected: str, error_source: SourceUnit = None) -> Optional[str]:
        """
        'Eats' next piece of text if it's equal to expected.

        :param expected: Value to be eaten.
        :return: Eaten value
        """
        if len(expected) == 0:
            self._move()
            return self.c

        for value in expected:
            nxt = self.peek(len(value))
            if nxt == value:
                self._move(len(nxt))
                return nxt
        if error_source is not None:
            error_source.blame(
                f'Expected {expected}',
                Span(
                    error_source,
                    self.location,
                    Location(self.location.line, self.location.column + 1)
                )
            )
        return None

    def _move(self, by: int = 1):
        assert (by != 0)
        if by > 0:
            while by > 0 and self.peek() != spec.eoc:
                if self.c == '\n':
                    self.line_idx += 1
                    self.prev_line_len = self.column_idx
                    self.column_idx = 0
                else:
                    self.column_idx += 1
                self.char_idx += 1
                by -= 1
        else:
            while by < 0:
                self.char_idx -= 1
                if self.c == '\n':
                    self.line_idx -= 1
                    self.column_idx = self.prev_line_len
                else:
                    self.column_idx -= 1
                by += 1
