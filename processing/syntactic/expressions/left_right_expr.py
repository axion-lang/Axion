from __future__ import annotations

import abc

from processing.syntactic.expressions.expr import Expr


class LeftRightExpr(metaclass = abc.ABCMeta):
    @abc.abstractmethod
    def left(self): pass

    @abc.abstractmethod
    def right(self): pass

    def __init__(self, parent: Expr):
        super().__init__(parent)
