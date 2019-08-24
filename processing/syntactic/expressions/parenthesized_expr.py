from __future__ import annotations

from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.type_names import TypeName


class ParenthesizedExpr(Expr):
    """ parenthesis_expr:
        '(' expr ')';
    """

    @child_property
    def value(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.value.value_type

    def __init__(self, parent: Expr, value: Expr):
        super().__init__(parent)
        self.value = value
