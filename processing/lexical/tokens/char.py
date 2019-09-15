from __future__ import annotations

import specification as spec
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import Location, span_marker
from source import SourceUnit


class CharToken(Token):
    def __init__(
            self,
            source: SourceUnit,
            value = '',
            unclosed = False,
            start = Location(0, 0),
            end = Location(0, 0)
    ):
        super().__init__(source, TokenType.character, value, start = start, end = end)
        self.unclosed = unclosed

    @property
    def value_type(self):
        from processing.syntactic.expressions.type_names import SimpleTypeName
        return SimpleTypeName(self.source.ast, spec.char_type_name)

    @span_marker
    def read(self) -> CharToken:
        self.append_next(spec.character_quote, error_source = self.source)
        while not self.append_next(spec.character_quote):
            if self.stream.at_eol:
                self.source.blame(BlameType.unclosed_character_literal, self)
                self.unclosed = True
                return self
            if self.stream.peek_is(spec.escape_mark):
                self.read_escape_seq()
            else:
                self.append_next(content = True)
        if len(self.content) == 0:
            self.source.blame(BlameType.empty_character_literal, self)
        elif len(self.content.replace('\\', '')) != 1:
            self.source.blame(BlameType.character_literal_too_long, self)
        return self

    def to_axion(self, c: CodeBuilder):
        c += spec.character_quote, self.value
        if not self.unclosed:
            c += spec.character_quote

    def to_csharp(self, c: CodeBuilder):
        self.to_axion(c)

    def to_python(self, c: CodeBuilder):
        self.to_axion(c)
