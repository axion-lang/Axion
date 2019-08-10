from __future__ import annotations

from processing.lexical.tokens.operator import OperatorToken
from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import InfixExpression, StatementExpression


class UnaryExpr(InfixExpression, StatementExpression):
    """unary_expr:
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
