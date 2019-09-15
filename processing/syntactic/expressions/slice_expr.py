from __future__ import annotations

from processing.codegen.code_builder import CodeBuilder
from processing.syntactic.expressions.expr import Expr, child_property


class SliceExpr(Expr):
    """ slice_expr:
        [infix] ':' [infix] [':' [infix]];
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

    def to_axion(self, c: CodeBuilder):
        c += self.start, ':', self.stop
        if self.step is not None:
            c += ':', self.step

    def to_python(self, c: CodeBuilder):
        c += self.start, ':', self.stop, ':', self.step
