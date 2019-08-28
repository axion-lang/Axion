from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression
from processing.location import span_marker


class YieldExpr(StatementExpression):
    """ yield_expr:
        'yield' ('from' infix_expr) | infix_list;
    """

    @child_property
    def value(self) -> Expr:
        pass

    @property
    def is_yield_from(self):
        return self.from_token is not None

    def __init__(
            self,
            parent: Expr = None,
            yield_token: Token = None,
            from_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.yield_token = yield_token
        self.value = value
        self.from_token = from_token

    @span_marker
    def parse(self) -> YieldExpr:
        self.yield_token = self.stream.eat(TokenType.keyword_yield)
        if self.stream.maybe_eat(TokenType.keyword_from):
            self.from_token = self.stream.token
            self.value = self.parse_infix()
        else:
            self.value = self.parse_multiple(self.parse_infix)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.yield_token
        if self.is_yield_from:
            c += self.from_token
        c += self.value

    def to_csharp(self, c: CodeBuilder):
        c += 'yield '
        if self.is_yield_from:
            raise NotImplementedError
        c += self.value

    def to_python(self, c: CodeBuilder):
        c += 'yield '
        if self.is_yield_from:
            c += 'from '
        c += self.value
