from __future__ import annotations

import re
from typing import Optional

import specification as spec
import utils
from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.location import Span, Location, span_marker, FreeSpan
from source import SourceUnit


class Token(Span):
    """ Base class of all language tokens.
        Represents simple tokens, with empty 'content'.
        All it's inheritors have 'content' property
        differing from 'value', or have special properties.
    """

    def __init__(
            self,
            source: SourceUnit,
            ttype: TokenType = TokenType.unknown,
            value = '',
            content = '',
            start: Location = Location(0, 0),
            end: Location = Location(0, 0)
    ):
        super().__init__(source, start, end)
        if source is not None:
            self.tokens = source.token_stream.tokens
            self.stream = source.text_stream
        self.ttype = ttype
        self.value = value
        self.content = content
        self.ending_white = ''

    @property
    def value_type(self):
        from processing.syntactic.expressions.type_names import SimpleTypeName
        if self.ttype in [TokenType.keyword_true, TokenType.keyword_false]:
            return SimpleTypeName(self.source.ast, spec.bool_type_name)
        if self.ttype == TokenType.keyword_nil:
            return SimpleTypeName(self.source.ast, spec.nil_type_name)
        return

    def of_type(self, *ttypes: TokenType) -> bool:
        """
        Checks if token's type equal to
        one of specified types.
        """
        return self.ttype in ttypes

    def append_next(self, *expected: str, content = False, error_source: SourceUnit = None) -> bool:
        """
        Appends next eaten string to token value.

        :param expected: Values expected to eat.
        :param content: If true, value is also appended to token's 'content'.
        :param error_source: If specified, error is thrown when
        next value is not in expected values.
        """
        eaten = self.source.text_stream.eat(*expected, error_source = error_source) or ''
        self.value += eaten
        if content:
            self.content += eaten
        return eaten != ''

    @span_marker
    def read(self) -> Token:
        """ Reads any token from text stream.
        """
        from processing.lexical.tokens.comment import CommentToken
        from processing.lexical.tokens.char import CharToken
        from processing.lexical.tokens.string import StringToken
        from processing.lexical.tokens.number import NumberToken
        from processing.lexical.tokens.operator import OperatorToken

        # assert no 'holes' between neighbour token positions
        # if len(self.tokens) > 0 and self.tokens[-1] != TokenType.outdent:
        #     assert self.start == self.tokens[-1].end

        if self.stream.peek_is(*spec.white):
            return self.read_white()

        if self.stream.peek_is(*spec.punctuation_keys):
            return self.read_punctuation()

        if self.stream.peek_is(*spec.operators_signs):
            return OperatorToken(self.source).read()

        if self.stream.peek_is(*spec.id_start):
            return self.read_id()

        if self.stream.peek_is(*spec.number_start):
            return NumberToken(self.source).read()

        if self.stream.peek_is(spec.multiline_comment_mark):
            return CommentToken(self.source).read_multiline()

        if self.stream.peek_is(spec.oneline_comment_mark):
            return CommentToken(self.source).read_oneline()

        if self.stream.peek_is(spec.character_quote):
            return CharToken(self.source).read()

        if self.stream.peek_is(*spec.string_quotes):
            return StringToken(self.source).read()

        if self.stream.peek_is(*spec.eols):
            return self.read_newline()

        if self.stream.peek_is(spec.eoc):
            return self.read_eoc()

        # invalid character token
        self.ttype = TokenType.invalid
        self.append_next()
        self.source.blame(BlameType.invalid_character, self)
        return self

    @span_marker
    def read_newline(self) -> Token:
        self.ttype = TokenType.newline
        while self.append_next(*spec.eols):
            pass
        if self.stream.peek_is(*spec.white):
            return self
        # root-level newline - reset indentation to 0
        self.tokens.append(self)
        self.source.last_indent_len = 0
        while self.source.indent_level > 0:
            self.tokens.append(Token(self.source, TokenType.outdent, start = self.stream.location))
            self.source.indent_level -= 1

    @span_marker
    def read_white(self) -> Optional[Token]:
        from processing.lexical.tokens.operator import OperatorToken, InputSide

        while self.append_next(*spec.white):
            pass
        if len(self.tokens) == 0:
            self.source.last_indent_len = len(self.value)
            self.ttype = TokenType.whitespace
            return self
        ln = self.stream.rest_of_line

        # check for indentation beginning
        if self.tokens[-1].of_type(TokenType.newline) \
                and not (
                # line empty
                len(ln.strip()) == 0
                # prev token is bin operator
                or isinstance(self.tokens[-1], OperatorToken) and self.tokens[-1].input_side == InputSide.both
                # line commented
                or ln.startswith(spec.oneline_comment_mark) or (
                        ln.startswith(spec.multiline_comment_mark) and
                        spec.multiline_comment_mark in ln
                )
                # next token is bin operator
                or any(re.match(fr'^{re.escape(op)}[^{spec.id_start}]', ln) for op in spec.operators_keys)
                or len(self.source.mismatching_pairs) > 0
        ):
            if self.source.indent_char == '\0':
                self.source.indent_char = self.value[0]
            consistent = True
            inconsistency_start: Optional[Location] = None
            new_indent_len = 0

            for i, ch in enumerate(self.value):
                if consistent and ch != self.source.indent_char:
                    inconsistency_start = Location(self.start.line, i)
                    consistent = False

                if ch == ' ':
                    new_indent_len += 1
                elif ch == '\t':
                    new_indent_len += self.source.indent_size or 8

            if consistent:
                if self.source.indent_size == 0:
                    self.source.indent_size = new_indent_len
            else:  # TODO elif unit.Options.HasFlag(CheckIndentConsistency)
                self.source.blame(
                    BlameType.inconsistent_indentation,
                    FreeSpan(self.source, inconsistency_start, self.end)
                )

            if new_indent_len > self.source.last_indent_len:
                self.ttype = TokenType.indent
                self.source.indent_level += 1
                result = self
            else:
                self.ttype = TokenType.outdent
                while new_indent_len < self.source.last_indent_len:
                    self.tokens.append(self)
                    self.source.indent_level -= 1
                    self.source.last_indent_len -= self.source.indent_size
                result = None

            self.source.last_indent_len = new_indent_len
            return result
        else:
            self.tokens[-1].ending_white += self.value
        return None

    @span_marker
    def read_eoc(self) -> Token:
        self.ttype = TokenType.end
        self.append_next(spec.eoc)
        return self

    @span_marker
    def read_id(self) -> Token:
        while True:
            self.append_next()
            if not self.stream.peek_is(*spec.id_part) \
                    or self.stream.peek_is(*spec.id_not_end) \
                    and self.stream.peek(2)[1] not in spec.id_after_not_end:
                break
        if self.stream.peek_is(*spec.string_quotes):
            from processing.lexical.tokens.string import StringToken
            return StringToken(self.source, prefixes = self.value).read()
        if self.value in spec.keywords.keys():
            self.ttype = spec.keywords[self.value]
        elif self.value in spec.operators_keys:
            from processing.lexical.tokens.operator import OperatorToken
            return OperatorToken(self.source, self.value, start = self.start, end = self.end)
        else:
            self.ttype = TokenType.identifier
        return self

    @span_marker
    def read_punctuation(self) -> Token:
        self.append_next(*spec.punctuation_keys)
        self.ttype = spec.punctuation[self.value]
        mismatches = self.source.mismatching_pairs
        if self.ttype in spec.open_brackets:
            mismatches.append(self)
        elif self.ttype in spec.close_brackets:
            if len(mismatches) > 0 and spec.matching_bracket(mismatches[-1].ttype) == self.ttype:
                mismatches.pop()
            else:
                mismatches.append(self)
        return self

    def read_escape_seq(self):
        def raise_error(blame_type: BlameType):
            self.source.blame(
                blame_type,
                Token(source = self.source, start = escape_start, end = self.stream.location)
            )
            self.value += raw
            self.content += raw

        raw = escaped = self.stream.eat(spec.escape_mark)
        escape_start = self.stream.location
        # \t, \n, etc.
        if self.stream.eat(*spec.escape_sequences.keys()):
            raw += self.stream.c
            escaped += spec.escape_sequences[self.stream.c]
        # \u h{4} or \U h{8}
        elif self.stream.eat('u', 'U'):
            u = self.stream.c
            raw += u
            u_escape_len = 4 if u == 'u' else 8
            escape_len = 0
            while escape_len < u_escape_len:
                if self.stream.eat(*spec.number_hex):
                    raw += self.stream.c
                    escape_len += 1
                else:
                    raise_error(BlameType.truncated_escape_sequence)
                    return
            num = utils.parse_int(raw[2:], 16)
            if num != 0:
                if 0 <= num <= 0x10ffff:
                    escaped += chr(num)
                else:
                    raise_error(BlameType.illegal_unicode_character)
                    return
            else:
                raise_error(BlameType.invalid_escape_sequence)
                return
        # \xhh
        elif self.stream.eat('x'):
            raw += self.stream.c
            while self.stream.eat(*spec.number_hex) and len(raw) < 4:
                raw += self.stream.c
            if len(raw) != 4:
                raise_error(BlameType.invalid_x_escape_format)
                return
            num = utils.parse_int(raw[2:], 16)
            if num != 0:
                escaped += chr(num)
            else:
                raise_error(BlameType.invalid_escape_sequence)
                return
        elif self.stream.at_eol:
            raise_error(BlameType.truncated_escape_sequence)
            return
        else:
            raise_error(BlameType.invalid_escape_sequence)
            return
        self.value += raw
        self.content += escaped

    def to_axion(self, c: CodeBuilder):
        c += self.value

    def to_csharp(self, c: CodeBuilder):
        c += self.value

    def to_python(self, c: CodeBuilder):
        c += self.value
