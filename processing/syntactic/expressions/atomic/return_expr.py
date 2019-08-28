from __future__ import annotations

import specification as spec
from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.lexical.tokens.token_type import TokenType
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import StatementExpression, AtomExpression
from processing.location import span_marker


class ReturnExpr(AtomExpression, StatementExpression):
    """ return_expr:
        'return' [preglobal_list];
    """

    @child_property
    def value(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            return_token: Token = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.return_token = return_token
        self.value = value

    @span_marker
    def parse(self) -> ReturnExpr:
        self.return_token = self.stream.eat(TokenType.keyword_return)
        if not self.stream.peek.of_type(*spec.never_expr_start_types):
            self.value = self.parse_multiple()
        # TODO: check for current fn
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.return_token, self.value

    def to_csharp(self, c: CodeBuilder):
        c += 'return ', self.value

    def to_python(self, c: CodeBuilder):
        c += 'return ', self.value
