from __future__ import annotations

from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import InfixExpression
from processing.syntactic.expressions.type_names import TypeName


class ConditionalInfixExpr(InfixExpression):
    """conditional_infix_expr:
       expr_list ('if' | 'unless') infix_expr ['else' expr_list];
    """

    @child_property
    def condition(self) -> Expr:
        pass

    @child_property
    def true_expression(self) -> Expr:
        pass

    @child_property
    def false_expression(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.true_expression.value_type

    def __init__(
            self,
            parent: Expr = None,
            condition: Expr = None,
            true_expression: Expr = None,
            false_expression: Expr = None
    ):
        super().__init__(parent)
        self.condition = condition
        self.true_expression = true_expression
        self.false_expression = false_expression
