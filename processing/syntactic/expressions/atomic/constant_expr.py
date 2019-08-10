from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.token import Token
from processing.syntactic.expressions.expr import Expr
from processing.syntactic.expressions.expression_groups import AtomExpression
from processing.text_location import span_marker


class ConstantExpr(AtomExpression):
    """const_expr:
       CONST_TOKEN;
    """

    def __init__(
            self,
            parent: Expr = None,
            literal: Token = None
    ):
        super().__init__(parent)
        self.literal = literal

    @span_marker
    def parse(self) -> ConstantExpr:
        self.literal = self.stream.eat_any()
        return self

    def to_axion(self, c: CodeBuilder):
        c += self.literal

    def to_csharp(self, c: CodeBuilder):
        c += self.literal
