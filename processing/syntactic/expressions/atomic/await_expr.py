from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression, AtomExpression
from processing.location import span_marker


class AwaitExpr(AtomExpression, StatementExpression):
    """ await_expr:
        'await' expr_list;
    """

    @child_property
    def value(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            await_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.await_token = await_token
        self.value = value

    @span_marker
    def parse(self) -> AwaitExpr:
        self.await_token = self.stream.eat(TokenType.keyword_await)
        self.value = self.parse_multiple(self.parse_any)
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.await_token, self.value

    def to_csharp(self, c: CodeBuilder):
        c += 'await (', self.value, ')'

    def to_python(self, c: CodeBuilder):
        c += 'await ', self.value
