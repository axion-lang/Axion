from __future__ import annotations

from typing import List

import specification as spec
import utils
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.text_stream import TextStream
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import Location, span_marker, FreeSpan
from source import SourceUnit


class StringToken(Token):
    def __init__(
            self,
            source: SourceUnit,
            value = '',
            unclosed = False,
            prefixes = '',
            quote = '"',
            eols_normalize = False,
            start = Location(0, 0),
            end = Location(0, 0),
    ):
        super().__init__(source, TokenType.string, value, start = start, end = end)
        self.unclosed = unclosed
        self.prefixes = prefixes
        self.quote = quote
        self.eols_normalize = eols_normalize
        self.ending_quotes = ''
        self.interpolations: List[StringInterpolation] = []

    @property
    def is_multiline(self) -> bool:
        return len(self.quote) == 3

    def has_prefix(self, prefix: str) -> bool:
        return prefix.lower() in self.prefixes \
               or prefix.upper() in self.prefixes

    @span_marker
    def read(self) -> StringToken:
        self.append_next(*spec.string_quotes, error_source = self.source)
        self.quote = self.value[0]
        # """ or '''
        if self.append_next(self.quote * 2):
            self.quote *= 3
        # empty string ("" | '')
        elif self.append_next(self.quote):
            if len(self.prefixes) > 0:
                self.source.blame(BlameType.redundant_prefixes_for_empty_string, self)
            return self
        while True:
            if self.stream.peek_is(spec.escape_mark) and not self.has_prefix('r'):
                self.read_escape_seq()
            elif (self.stream.peek_is('\r', '\n') and not self.is_multiline) or self.stream.peek_is(spec.eoc):
                self.unclosed = True
                self.source.blame(BlameType.unclosed_string, self)
                break
            elif self.stream.eat('\r\n') and self.eols_normalize:
                # \r\n -> \n
                self.value += '\n'
                self.content += '\n'
            elif self.append_next(self.quote[0]):
                closing_quote = self.quote[0]
                if len(self.quote) == 1:
                    # closing single
                    break
                if self.append_next(closing_quote * 2):
                    # closing triple
                    break
                # trailing quotes (unclosed string)
                closing_loc = self.stream.location
                self.ending_quotes += closing_quote
                if self.append_next(closing_quote):
                    # two trailing quotes
                    self.ending_quotes += closing_quote
                self.source.blame(
                    BlameType.unescaped_quote_in_string_literal,
                    FreeSpan(self.source, closing_loc, self.stream.location)
                )
                break
            elif self.has_prefix('f') and self.stream.peek_is('{'):
                self.interpolations.append(StringInterpolation(self.stream).read())
            else:
                self.append_next(content = True)
        if self.has_prefix('f') and len(self.interpolations) == 0:
            self.source.blame(BlameType.redundant_string_format)
        return self

    def to_csharp(self, c: CodeBuilder):
        if self.has_prefix('f'):
            c += '$'
        if self.has_prefix('r') or self.is_multiline:
            c += '@'
        c += '"', self.content
        if not self.unclosed:
            c += '"'


class StringInterpolation(utils.AutoRepr):
    def __init__(self, stream: TextStream):
        self.source = SourceUnit.from_interpolation(stream)
        self.source.process_terminators.append(TokenType.close_brace)

    def read(self) -> StringInterpolation:
        from compiler import Compiler
        Compiler.lexical_analysis(self.source)
        # remove '{' '}'
        self.source.token_stream.tokens.pop(0)
        self.source.token_stream.tokens.pop()
        return self
