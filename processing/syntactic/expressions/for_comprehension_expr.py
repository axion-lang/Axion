from __future__ import annotations

from processing.syntactic.expressions.expr import Expr, child_property
from processing.syntactic.expressions.expression_groups import GlobalExpression
from processing.syntactic.expressions.type_names import TypeName


class ForComprehensionExpr(GlobalExpression):
    """for_comprehension:
       'for' simple_name_list 'in' preglobal_expr [comprehension];
    """

    @child_property
    def target(self) -> Expr:
        pass

    @child_property
    def item(self) -> Expr:
        pass

    @child_property
    def iterable(self) -> Expr:
        pass

    @child_property
    def right(self) -> Expr:
        pass

    @property
    def value_type(self) -> TypeName:
        return self.target.value_type

    def __init__(
            self,
            parent: Expr = None,
            target: Expr = None,
            item: Expr = None,
            iterable: Expr = None,
            right: Expr = None
    ):
        super().__init__(parent)
        self.target = target
        self.item = item
        self.iterable = iterable
        self.right = right
