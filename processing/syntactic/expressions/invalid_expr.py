from __future__ import annotations

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import span_marker
from processing.syntactic.expressions.expr import Expr


class InvalidExpr(Expr):
    """ expr:
        TOKEN* (NEWLINE | END);
    """

    @property
    def text(self):
        return ''.join(t.value + t.ending_white for t in self.tokens)

    def __init__(
            self,
            parent: Expr = None,
            *tokens: Token
    ):
        super().__init__(parent)
        self.tokens = list(tokens)
        self.source.blame(BlameType.invalid_syntax, self)

    @span_marker
    def parse(self) -> InvalidExpr:
        while not self.stream.exact_peek.of_type(TokenType.newline, TokenType.end):
            self.stream.eat_any()
            self.tokens.append(self.stream.token)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.text

    def to_python(self, c: CodeBuilder):
        self.to_axion(c)

    def to_csharp(self, c: CodeBuilder):
        self.to_axion(c)
