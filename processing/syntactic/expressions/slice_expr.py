from __future__ import annotations

from processing.syntactic.expressions.expr import Expr, child_property


class SliceExpr(Expr):
    """ slice_expr:
        [preglobal_expr] ':' [preglobal_expr] [':' [preglobal_expr]];
    """

    @child_property
    def start(self) -> Expr:
        pass

    @child_property
    def stop(self) -> Expr:
        pass

    @child_property
    def step(self) -> Expr:
        pass

    def __init__(
            self,
            parent: Expr = None,
            start: Expr = None,
            stop: Expr = None,
            step: Expr = None
    ):
        super().__init__(parent)
        self.start = start
        self.stop = stop
        self.step = step
