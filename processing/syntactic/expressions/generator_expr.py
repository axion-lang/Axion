from __future__ import annotations

from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import GlobalExpression
from processing.syntactic.expressions.for_comprehension_expr import ForComprehensionExpr
from processing.syntactic.expressions.type_names import TypeName


class GeneratorExpr(GlobalExpression):
    """generator_expr:
       '(' comprehension ')';
    """

    @child_property
    def comprehension(self) -> ForComprehensionExpr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.comprehension.value_type

    def __init__(
            self,
            parent: Expr,
            comprehension: ForComprehensionExpr
    ):
        super().__init__(parent)
        self.comprehension = comprehension
