from __future__ import annotations

from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr


class UnknownExpr(Expr):
    """expr:
       TOKEN* (NEWLINE | END);
    """

    def __init__(
            self,
            parent: Expr = None,
            *tokens: Token
    ):
        super().__init__(parent)
        self.tokens = list(tokens)

    def parse(self) -> UnknownExpr:
        while not self.stream.peek.of_type(TokenType.newline, TokenType.end):
            self.tokens.append(self.stream.eat_any())
        return self
