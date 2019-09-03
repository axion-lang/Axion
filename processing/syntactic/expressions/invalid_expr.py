from __future__ import annotations

from errors.blame import BlameType
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.location import span_marker
from processing.syntactic.expressions.expr import Expr


class InvalidExpr(Expr):
    """ expr:
        TOKEN* (NEWLINE | END);
    """

    def __init__(
            self,
            parent: Expr = None,
            *tokens: Token
    ):
        super().__init__(parent)
        self.text = ''.join(t.value + t.ending_white for t in tokens)
        self.source.blame(BlameType.invalid_syntax, self)

    @span_marker
    def parse(self) -> InvalidExpr:
        while not self.stream.exact_peek.of_type(TokenType.newline, TokenType.end):
            self.stream.eat_any()
            self.text += self.stream.token.value + self.stream.token.ending_white
        return self
