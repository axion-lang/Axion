from __future__ import annotations

from errors.blame import BlameType
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr
from processing.location import span_marker


class UnknownExpr(Expr):
    """ expr:
        TOKEN* (NEWLINE | END);
    """

    def __init__(
            self,
            parent: Expr = None,
            *tokens: Token
    ):
        super().__init__(parent)
        self.tokens = list(tokens)
        self.source.blame(BlameType.invalid_syntax, self)

    @span_marker
    def parse(self) -> UnknownExpr:
        while not self.stream.exact_peek.of_type(TokenType.newline, TokenType.end):
            self.tokens.append(self.stream.eat_any())
        return self
