from __future__ import annotations

from typing import Optional

from errors.blame import BlameType
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.slice_expr import SliceExpr
from processing.syntactic.parsing import parse_infix, parse_atom, maybe_tuple


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
            self.target = parse_atom(self)
        exprs = []
        self.stream.eat(TokenType.open_bracket)
        while True:
            start: Optional[Expr] = None
            if not self.stream.peek.of_type(TokenType.colon):
                start = parse_infix(self)
            if self.stream.maybe_eat(TokenType.colon):
                stop: Optional[Expr] = None
                if not self.stream.peek.of_type(
                        TokenType.colon,
                        TokenType.comma,
                        TokenType.close_bracket
                ):
                    stop = parse_infix(self)
                step: Optional[Expr] = None
                if self.stream.maybe_eat(TokenType.colon) \
                        and not self.stream.peek.of_type(
                    TokenType.comma,
                    TokenType.close_bracket
                ):
                    step = parse_infix(self)
                exprs.append(SliceExpr(self, start, stop, step))
            else:
                if start is None:
                    self.source.blame(BlameType.invalid_indexer_expression, self.stream.token)
                exprs.append(start)
            if self.stream.maybe_eat(TokenType.comma):
                continue
            self.stream.eat(TokenType.close_bracket)
            break
        self.index = maybe_tuple(self, exprs)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.target, '[', self.index, ']'

    def to_csharp(self, c: CodeBuilder):
        c += self.target, '[', self.index, ']'

    def to_python(self, c: CodeBuilder):
        c += self.target, '[', self.index, ']'
