from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.lexical.tokens.operator import OperatorToken, InputSide
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.groups import InfixExpression, StatementExpression


class UnaryExpr(InfixExpression, StatementExpression):
    """ unary_expr:
        UNARY_LEFT prefix_expr
        | suffix_expr UNARY_RIGHT;
    """

    @child_property
    def value(self) -> Expr: pass

    def __init__(
            self,
            parent: Expr = None,
            operator: OperatorToken = None,
            value: Expr = None
    ):
        super().__init__(parent)
        self.operator = operator
        self.value = value
        # TODO: mark span depending on input side

    def to_axion(self, c: CodeBuilder):
        if self.operator.input_side == InputSide.left:
            c += self.value, ' ', self.operator
        elif self.operator.input_side == InputSide.right:
            c += self.operator, ' ', self.value
        else:
            raise NotImplementedError

    def to_csharp(self, c: CodeBuilder):
        self.to_axion(c)

    def to_python(self, c: CodeBuilder):
        self.to_axion(c)
