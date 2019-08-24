from __future__ import annotations

from typing import Optional

from errors.blame import BlameType
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.slice_expr import SliceExpr


class IndexerExpr(Expr):
    """ indexer_expr:
        ;
    """

    @child_property
    def target(self) -> Expr:
        pass

    @child_property
    def index(self) -> Expr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            target: Expr = None,
            index: Expr = None
    ):
        super().__init__(parent)
        self.target = target
        self.index = index

    def parse(self) -> IndexerExpr:
        if self.target is None:
            self.target = self.parse_atom()
        exprs = []
        self.stream.eat(TokenType.open_bracket)
        while True:
            start: Optional[Expr] = None
            if not self.stream.peek.of_type(TokenType.colon):
                start = self.parse_infix()
            if self.stream.maybe_eat(TokenType.colon):
                stop: Optional[Expr] = None
                if not self.stream.peek.of_type(
                        TokenType.colon,
                        TokenType.comma,
                        TokenType.close_bracket
                ):
                    stop = self.parse_infix()
                step: Optional[Expr] = None
                if self.stream.maybe_eat(TokenType.colon) \
                        and not self.stream.peek.of_type(
                    TokenType.comma,
                    TokenType.close_bracket
                ):
                    step = self.parse_infix()
                exprs.append(SliceExpr(self, start, stop, step))
            if start is None:
                self.source.blame(BlameType.invalid_indexer_expression, self.stream.token)
            exprs.append(start)
            if self.stream.maybe_eat(TokenType.comma):
                continue
            self.stream.eat(TokenType.close_bracket)
            break
        self.index = self.maybe_tuple(exprs)
        return self
